terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  required_version = ">= 1.5.0"
}

provider "aws" {
  region = "eu-west-2" # your region
}

########################
# Network (default VPC)
########################

data "aws_vpc" "default" {
  default = true
}

data "aws_subnets" "default" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}

########################
# ECS Cluster
########################

resource "aws_ecs_cluster" "book_lending_cluster" {
  name = "book-lending-cluster"
}

########################
# IAM for ECS Task Execution
########################

data "aws_iam_policy_document" "ecs_task_assume" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "ecs_task_execution_role" {
  name               = "book-lending-ecs-task-execution-role"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_attach" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

########################
# Security Group
########################

resource "aws_security_group" "ecs_service_sg" {
  name        = "book-lending-ecs-sg"
  description = "Allow HTTP"
  vpc_id      = data.aws_vpc.default.id

 # Allow HTTP from internet to ALB
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Allow ALB (and Internet for now) to hit containers on 8080
  ingress {
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

########################
# Load Balancer
########################

resource "aws_lb" "app_lb" {
  name               = "book-lending-alb"
  load_balancer_type = "application"
  security_groups    = [aws_security_group.ecs_service_sg.id]
  subnets            = data.aws_subnets.default.ids
}

resource "aws_lb_target_group" "app_tg" {
  name     = "book-lending-tg"
  port     = 8080
  protocol = "HTTP"
  vpc_id   = data.aws_vpc.default.id

  target_type = "ip"

  health_check {
    path                = "/api/books"
    matcher             = "200-399"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
}

resource "aws_lb_listener" "http_listener" {
  load_balancer_arn = aws_lb.app_lb.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.app_tg.arn
  }
}

########################
# ECS Task Definition
########################

variable "ecr_image_uri" {
  type        = string
  description = "Full ECR image URI, e.g. 2367....dkr.ecr.eu-west-2.amazonaws.com/book-lending-api:latest"
}

resource "aws_ecs_task_definition" "book_lending_task" {
  family                   = "book-lending-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"

  execution_role_arn = aws_iam_role.ecs_task_execution_role.arn

  container_definitions = jsonencode([
    {
      name      = "book-lending-container"
      image     = var.ecr_image_uri
      essential = true
      portMappings = [
        {
          containerPort = 8080
          hostPort      = 8080
          protocol      = "tcp"
        }
      ]
      environment = [
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:8080"
        }
      ]
    }
  ])
}

########################
# ECS Service
########################

resource "aws_ecs_service" "book_lending_service" {
  name            = "book-lending-service"
  cluster         = aws_ecs_cluster.book_lending_cluster.id
  task_definition = aws_ecs_task_definition.book_lending_task.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets         = data.aws_subnets.default.ids
    security_groups = [aws_security_group.ecs_service_sg.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.app_tg.arn
    container_name   = "book-lending-container"
    container_port   = 8080
  }

  depends_on = [
    aws_lb_listener.http_listener
  ]
}

########################
# Outputs
########################

output "alb_dns_name" {
  description = "Public DNS name of the load balancer"
  value       = aws_lb.app_lb.dns_name
}
