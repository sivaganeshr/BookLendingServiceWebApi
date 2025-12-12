# Book Lending Service

This is a sample Book Lending API behalf of the assessment, built using .NET 8. It demonstrates Web API development,Code quality and structure, API design and usability, Environment setup and deployment readiness, data persistence using Entity Framework Core with SQLite, testing(unit, Integration), Docker containerization, and cloud deployment using Terraform and AWS ECS Fargate with an Application Load Balancer.

## Technologies Used

This application is built using .NET 8 Web API. Entity Framework Core is used for data access. SQLite is used as the local development database. Swagger is used for API documentation in development mode. Docker is used to containerize the application. Terraform is used for the AWS infrastructure. 

AWS ECR stores the Docker image, ECS Fargate runs the container, and an AWS Application Load Balancer exposes the API publicly.

## Design Patterns and Architecture

1. Controller -> Service -> Repository architecture. 
2. Dependency Injection. 
3. DTOs for request/response models. 
4. Middleware Pipeline. 
5. Correlation ID logging pattern  
6. Global Exception Handling 

## Required NuGet Packages

This project requires below Nuget Packges
### BookLendingService.Api
    - Microsoft.EntityFrameworkCore 
    - Microsoft.EntityFrameworkCore.Sqlite
    - Microsoft.EntityFrameworkCore.Design
    - Swashbuckle.AspNetCore.

### Test Project
    - xUnit
    - xunit.runner.visualstudio
    - Moq
    - FluentAssertions
    - Microsoft.NET.Test.Sdk
    - Microsoft.EntityFrameworkCore.InMemory

## Exception Handling

Added middleware to handle the exceptions globally.

## How to Run Local Development Environment

 1. Restore all dependencies by running:

    ```
    dotnet restore
    ```

 2. Apply EF Core migrations to create the SQLite database:

    ```
    cd BookLendingService.Api
    dotnet ef database update
    ```

 3. Run the API:

    ```
    dotnet run --project BookLendingService.Api
    ```

    The API will be available at:

    ```
    http://localhost:{port}
    https://localhost:{port}
    ```

 4. Swagger (only in Development):

    ```
    http://localhost:{port}/swagger
    https://localhost:{port}/swagger
    ```

## Running Tests

    Run all tests including unit and integration tests using:

    ```
    dotnet test
    ```

## How to Run the Docker Image

    Build the Docker image:

    ```
    docker build -t book-lending-api .
    ```

    Run the container:

    ```
    docker run -p 8080:8080 book-lending-api
    ```

    The API will be accessible at:

    ```
    http://localhost:8080/books
    ```

## How to Deploy in AWS

    Authenticate Docker with Amazon ECR:

    ```
    aws ecr get-login-password --region <region> |
    docker login --username AWS --password-stdin <account_id>.dkr.ecr.<region>.amazonaws.com
    ```

    Tag the image:

    ```
    docker tag book-lending-api:latest <account_id>.dkr.ecr.<region>.amazonaws.com/book-lending-api:latest
    ```

    Push the image:

    ```
    docker push <account_id>.dkr.ecr.<region>.amazonaws.com/book-lending-api:latest
    ```

    Deploy the infrastructure using Terraform:

    ```
    cd infra/terraform
    terraform init
    terraform apply
    ```

    After successful deployment, Terraform prints the Application Load Balancer DNS name(alb_dns_name). Open the API using:

    ```
    http://<alb_dns_name>/books
    ```

Additionally I added some result screen shots please go through it. 


