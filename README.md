# tvmaze-scraper


TVMaze Scraper is an application that scrapes data from the TVMaze API, stores it in a database, and provides it through a REST API that meets specific business requirements. This application scrapes TV show and cast information from the TVMaze database and provides a paginated list of all TV shows with their respective ID and a list of cast members ordered by their birthday in descending order. This application is designed to provide a user-friendly way to access TV show and cast information from the TVMaze database.

## Contents
- [Pre-requisites](#prerequisites)
- [Setup application and run](#setup-application-and-run)
- [Usage](#usage)
- [Testing](#testing)
- [System diagram](#system-diagram)


# Prerequisites
To build and run this application locally, you will need the following:

- Docker Compose: to run the application
- Node.js: to run the script for migrating the database and setting up ElasticSearch
- .NET Core (minimum version 6): to run unit tests
- Python 3 (minimum version 3.7): to run the load testing application
- Postman (or an alternative)

# Setup application and run
1. Clone the repository
```
git clone https://github.com/mabbadi/tvmaze-scraper
```
2. Install dependencies
```
cd tvmaze-scraper
cd rtl_app
npm install
```
3. Start the application using Docker Compose. The `--scale` option spawns multiple instances of the .NET backend to handle a higher load of requests.
```
docker-compose up --build --scale backend=2
```
4. Set up the databases
```
npm run es-update
npm run db-migrate
```
After this step is done, stop the containers and run `docker-compose up --scale backend=2` again.

5. The application contains no data and no URLs to process when it starts. A Hangfire job is scheduled at midnight to download the deltas of the missing movies in the database. You can either wait or force-load the movies' URLs by using the following POST endpoint. Once ready, a background job will take care of downloading the movies every 10 seconds.

```
http://localhost:4000/TvMaze/process-all-data?key=1234
```

6. Go to [http://localhost:4000](http://localhost:4000) to check that everything is running. You should receive a 200 message containing "Application running".

# Usage

The REST API provided by the application must meet two specific criteria:
1. It must present a paginated list of all TV shows, including their respective IDs and the cast members currently involved with each show.
2. The list of cast members should be sorted in descending order based on their dates of birth.

## Endpoints

### GET shows
```
/TvMaze/search/shows?q=:query&page=:page
```
This endpoint provides the list of the most relevant TV shows for the provided query:

- `:page`: the page to be presented (default value is **1**). The default size is **10**. Note that this value is fixed but can be exported to the API if necessary.
- `:query`: the query to base the search upon. The default is **empty** which translates to select all shows.

Example of queries:
- [http://localhost:4000/TvMaze/search/shows?q=American Dad&page=1](http://localhost:4000/TvMaze/search/shows?q=American%20Dad&page=1)
- [http://localhost:4000/TvMaze/search/shows?q=&page=1](http://localhost:4000/TvMaze/search/shows?q=&page=1)

# Testing

## Unit test
### xunit
To run xunit test:
```
cd tvmaze-scraper
cd rtl_app_xunit
dotnet test
```

## Load test
### Locust installation
The first time, go to:
```
cd tvmaze-scraper
cd locust_load_test
```
and run:
```
pip3 install locust
```

### Locust run
To run load testing:

```
cd tvmaze-scraper
cd locust_load_test
locust
```
Once Locust is running, open the URL shown in the terminal in your web browser and set up the desired parameters to run the test. The script we are using for this basic test is located in the root of the locust_load_test directory and is called `locustfile.py`.

# System diagram
The following diagram illustrates the basic structure of the application:

<img src="/application structure.jpg" alt="Alt text" title="Optional title">
