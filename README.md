# CAT (Computerized Adaptive Testing) POC
  # Intro
    CAT is an adaptive testing standard that seeks to calculate the skill level of a candidate by dynamically adapting the test based on previous answers.
    For more info check https://www.nclex.com/computerized-adaptive-testing.page
  # Running the app
    # PreRequisites
      - .net 8.0
    # Commands
      - Open the terminal, and navigate to the solution root path
      - Run `dotnet run -p Cat.Api/Cat.Api.csproj`
      - Open the browser and navigate to "http://localhost:5281/swagger/index.html"
      - Start a session
      - Answer questions using the submit API
      - End the session to get the score
