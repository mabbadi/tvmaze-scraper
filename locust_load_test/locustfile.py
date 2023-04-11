from locust import HttpUser, task

class MyUser(HttpUser):
    host = "http://localhost:4000"
    
    @task
    def search_shows(self):
        self.client.get("/TvMaze/search/shows?q=girl&page=1")