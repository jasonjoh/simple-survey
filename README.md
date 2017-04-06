# Implementing a survey with Outlook actionable messages and Office 365 Group connectors

In this exercise, we'll go step-by-step to implement a survey using Outlook actionable messages. The survey app will allow a user to send a multiple-choice question to users, who can respond right from within Outlook using actions on the message. The app will tally results and send them to an Office 365 group once the survey has closed.

You can access the code of the completed exercise by downloading the `completed-exercise` branch of this repository.

## Prerequisites

In order to complete this exercise, you'll need the following:

- Visual Studio 2015 or later.
- [Postman](https://www.getpostman.com/) (or a similar tool to send HTTP POST requests)
- [ngrok](https://ngrok.com/)
- An Office 365 account

## Configure the web app

In this section, we'll download the starter solution and configure it to send messages via the [Microsoft Graph](https://developer.microsoft.com/en-us/graph).

Download or clone the repository, then open **SimpleSurvey.sln** in Visual Studio. The solution is comprised of 3 projects:

| Project | Description |
|---|---|
| [MessageCard.csproj](MessageCard/MessageCard.csproj) | This is a simple class library to represent the [actionable message JSON format](https://dev.outlook.com/actions/reference). |
| [SimpleSurvey.csproj](SimpleSurvey/SimpleSurvey.csproj) | This is an MVC web app that sends messages via Microsoft Graph with actionable message cards. |
| [SurveyModels.csproj](SurveyModels/SurveyModels.csproj) | This is a simple class library with model classes that will be shared between the web app and the web API to be added later. |

Before you try this out, you need to configure an application ID and password. The app uses Azure's [OAuth 2.0 Authorization Code Flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-code) to get access tokens for the Microsoft Graph, which requires that you register the app to get an app ID and password.

### Register the app with Azure

1. Open a browser and browse to [https://apps.dev.microsoft.com/](https://apps.dev.microsoft.com/).
1. Sign in with an Office 365 account or an Outlook.com account, then select the **Add an app** button.
1. Enter a name for the registration and select **Create application**.
1. Locate the **Application Secrets** section and choose the **Generate New Password** button. Copy the password that is generated in the popup and save it somewhere.
1. Locate the **Platforms** section and choose **Add Platform**.
1. Choose **Web**, then enter `http://localhost:1956/` under **Redirect URIs**.
1. Choose **Save** to finalize the registration. Copy the value of **Application Id** and save it somewhere.

### Add the values to the code

1. In Visual Studio, expand the **SimpleSurvey** project and open the **Web.config** file.
1. Find the following lines:

    ```xml
    <add key="ida:AppId" value="" />
    <add key="ida:AppSecret" value="" />
    ```
1. Set the value of the `ida:AppId` key to the application ID you generated in the app portal.
1. Set the value of the `ida:AppSecret` key to the application password you generated in the app portal.

### Test the app

Save your changes and press **F5** to build and run the app. A browser should open to the home page which asks you to sign in to create a survey. Sign in with an Office 365 account and grant access to the app. Once you're signed in, you should see a form that allows you to create and send a survey. Fill in the form and add a few choices, then choose **Send this poll**.

![A screenshot of the web app form](readme-images/send-survey.PNG)

Log in to Outlook on the web with the same account. You should have a new message with a rendered message card.

![A screenshot of the message card](readme-images/survey.PNG)

The **Submit** action doesn't work yet, and the survey is really just a placeholder that isn't saved anywhere. By the end of this exercise we'll change that.

---

## Adding a Web API

In this section, we'll add a Web API to the solution, which will allow the web app to create surveys in a database.

### Create the project

1. Add a new project to the solution. Choose **ASP.NET Web Application (.NET Framework)** under **Visual C#**. Name the project `SurveyService` and click **OK**.
1. In the next dialog, select **Empty**, and check the box under **Add folders and core references for:** for **Web API**. Make sure the **Host in the cloud** checkbox is not checked and click **OK**.
1. In **Solution Explorer**, right-click the **References** folder under the **SurveyService** project and choose **Add Reference...**. On the left-hand side expand **Projects** and select **Solution**. Check the boxes next to **SurveyModels** and **MessageCard**, then click **OK**.
1. On the **Tools** menu, choose **NuGet Package Manager**, then **Manage NuGet Packages for Solution...**. Click the **Browse** tab and search for `EntityFramework`. Select **EntityFramework** in the list of packages, then put a check in the box next to the **SurveyService** project. Click **Install**.

### Add the CreateSurveyRequest and CreateSurveyResponse models

1. Right-click the **SurveyModels** project and choose **Add**, then **Class**. Name the class `CreateSurveyRequest`. Repeat these steps to create a `CreateSurveyResponse` class.
1. Open **CreateSurveyRequest.cs** and update the class with the following code.

    ```C#
    class CreateSurveyRequest
    {
        public Survey Survey { get; set; }
        public string Sender { get; set; }
        public string[] Recipients { get; set; }
    }
    ```
1. Open **CreateSurveyResponse.cs** and update the class with the following code.

    ```C#
    public class SurveyParticipant
    {
        public string Email { get; set; }
        public string LimitedToken { get; set; }
    }

    public class CreateSurveyResponse
    {
        public string Status { get; set; }
        public int SurveyId { get; set; }
        public DateTime Expiration { get; set; }
        public List<SurveyParticipant> Participants { get; set; }
    }
    ```
1. Build the solution before proceeding in order to generate the models.

### Add the database models

In this section we'll create models for surveys, participants, and responses. These models will be used with the Entity Framework to make it easy to store in a local SQL database.

1. Right-click the **Models** folder in the **SurveyService** project and choose **Add**, then **Class**. Name the class `SimpleSurvey` and click **Add**. Repeat these steps to add a `Particpant` class, a `Response` class, and a `SurveyContext` class.
1. Open the **SimpleSurvey.cs** file and replace the entire contents of the file with the follwing code.

    ```C#
    using System;
    using System.Collections.Generic;

    namespace SurveyService.Models
    {
        public class SimpleSurvey
        {
            public int SimpleSurveyId { get; set; }
            public string Sender { get; set; }
            public string Name { get; set; }
            public DateTime Expiration { get; set; }
            public string QuestionTitle { get; set; }
            public string QuestionChoices { get; set; }
            public bool ResultsReported { get; set; }
            public virtual List<Participant> Participants { get; set; }
            public virtual List<Response> Responses { get; set; }
        }
    }
    ```
1. Open the **Participant.cs** file and replace the entire contents of the file with the follwing code.

    ```C#
    using System.Collections.Generic;

    namespace SurveyService.Models
    {
        public class Participant
        {
            public int ParticipantId { get; set; }
            public string Email { get; set; }
            public string TokenSalt { get; set; }
            public virtual List<SimpleSurvey> Surveys { get; set; }
            public virtual List<Response> Responses { get; set; }
        }
    }
    ```
1. Open the **Response.cs** file and replace the entire contents of the file with the follwing code.

    ```C#
    namespace SurveyService.Models
    {
        public class Response
        {
            public int ResponseId { get; set; }
            public virtual SimpleSurvey Survey { get; set; }
            public virtual Participant Participant { get; set; }
            public string ParticipantResponse { get; set; }
        }
    }
    ```
1. Open the **SurveyContext.cs** file and replace the entire contents of the file with the follwing code.

    ```C#
    using System.Data.Entity;

    namespace SurveyService.Models
    {
        public class SurveyContext : DbContext
        {
            public DbSet<SimpleSurvey> Surveys { get; set; }
            public DbSet<Participant> Participants { get; set; }
            public DbSet<Response> Responses { get; set; }
        }
    }
    ```
1. The last step to configure our database is to specify where we want the database file created. For this project, we'll tell the Entity Framework to create it in the **App_Data** folder in the **SurveyService** project. Open the **Web.config** file in the **SurveyService** project. Add the following code before the `<appSettings>` node:

    ```xml
    <connectionStrings>
      <add name="SurveyContext" connectionString="Data Source=(LocalDb)\v11.0;Initial Catalog=SurveyDatabase;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\SurveyDatabase.mdf" providerName="System.Data.SqlClient"/>
    </connectionStrings>
    ```

### Add the SurveysController

1. Right-click the **Controllers** folder under the **SurveyService** project and choose **Add**, then **Controller**. Choose **Web API 2 Controller - Empty** and click **Add**. Name the controller **SurveysController** and click **Add**.
1. Open the **SurveysController.cs** file and add the following `using` directives at the top of the file.

    ```C#
    using SurveyModels;
    using SurveyService.Models;
    ```
1. Add the following method to the `SurveysController` class.

    ```C#
    public IHttpActionResult PostSurvey(CreateSurveyRequest surveyRequest)
    {
        string newSurveyLocation = string.Empty;
        CreateSurveyResponse response = new CreateSurveyResponse();

        try
        {
            using (var db = new SurveyContext())
            {
                // Create the survey
                SimpleSurvey newSurvey = new SimpleSurvey
                {
                    Name = surveyRequest.Survey.Name,
                    Sender = surveyRequest.Sender,
                    Expiration = DateTime.UtcNow.AddMinutes(surveyRequest.Survey.Duration),
                    QuestionTitle = surveyRequest.Survey.QuestionTitle,
                    QuestionChoices = surveyRequest.Survey.QuestionChoices,
                    ResultsReported = false,
                    Participants = new List<Participant>()
                };

                // Create participants
                foreach (string recipient in surveyRequest.Recipients)
                {
                    // Check if recipient is already in database, create if not
                    var participant = db.Participants.FirstOrDefault(p => p.Email == recipient.ToLower()) ??
                        new Participant
                        {
                            Email = recipient.ToLower(),
                            TokenSalt = "" // TODO: Generate a per-user salt here
                        };

                    newSurvey.Participants.Add(participant);

                    // Add participant to database
                    db.Participants.Add(participant);
                }

                // Add survey to the database
                db.Surveys.Add(newSurvey);
                db.SaveChanges();

                newSurveyLocation = Url.Link("DefaultApi", new { id = newSurvey.SimpleSurveyId });

                response.Status = "Succeeded";
                response.SurveyId = newSurvey.SimpleSurveyId;
                response.Expiration = newSurvey.Expiration;
                response.Participants = new List<SurveyParticipant>();

                var rng = new RNGCryptoServiceProvider();

                foreach (Participant participant in newSurvey.Participants)
                {
                    response.Participants.Add(new SurveyParticipant
                    {
                        Email = participant.Email,
                        LimitedToken = "" // TODO: Generate a limited-purpose token here
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Created(newSurveyLocation, response);
    }
    ```

Let's take a look at what this code does. The `PostSurvey` method implements a `POST` method for surveys. A client can `POST` a serialized `CreateSurveyRequest` object to the API. The method will create the survey in the database. It then creates a participant record in the database for each recipient (or reuses an existing record). If this succeeds, it returns the survey ID, expiration date/time, and a list of participants to the caller.

However, this isn't quite complete! There are a couple of places marked with `TODO` comments. The first one is when the code creates a new record in the participant table. The user needs to have a token salt generated. The second one is when building the response. Each participant needs to have a limited-purpose token generated and added to the response. This is to meet the security requirements documented on [dev.outlook.com](https://dev.outlook.com/Actions/security-requirements). The caller can use these limited-purpose tokens in the action URLs in the actionable message.

### Add a TokenGenerator utility class

1. Right-click the **SurveyService** project and choose **Add**, then **New Folder**. Name the folder `Tokens`.
1. Right click the **Tokens** folder and choose **Add**, then **Class...**. Name the class `TokenGenerator` and click **Add**.
1. Replace the entire contents fo the **TokenGenerator.cs** file with the following code.

    ```C#
    using System;
    using System.Security.Cryptography;

    namespace SurveyService.Tokens
    {
        public static class TokenGenerator
        {
            private const int SaltSize = 32;
            private const int TokenSize = 32;
            private const int Iterations = 1000;

            public static string GenerateNewTokenSalt()
            {
                var rng = new RNGCryptoServiceProvider();
                byte[] randomBytes = new byte[SaltSize];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }

            public static string GenerateLimitedToken(int surveyId, string email, string salt)
            {
                byte[] saltBytes = Convert.FromBase64String(salt);
                var pbkdf2 = new Rfc2898DeriveBytes(string.Format("{0}{1}", surveyId, email), saltBytes, Iterations);
                byte[] hashBytes = pbkdf2.GetBytes(TokenSize);

                return Convert.ToBase64String(hashBytes);
            }

            public static bool VerifyLimitedToken(string token, int surveyId, string email, string salt)
            {
                return (token == GenerateLimitedToken(surveyId, email, salt));
            }
        }
    }
    ```
1. Open the **SurveysController.cs** file and locate the line:

    ```C#
    TokenSalt = "" // TODO: Generate a per-user salt here
    ```

    Replace this line with the following:

    ```C#
    TokenSalt = TokenGenerator.GenerateNewTokenSalt()
    ```

2. Locate the line:

    ```C#
    LimitedToken = "" // TODO: Generate a limited-purpose token here
    ```

    Replace this line with the following:

    ```C#
    LimitedToken = TokenGenerator.GenerateLimitedToken(newSurvey.SimpleSurveyId, participant.Email, participant.TokenSalt)
    ```

### Test creating a survey

Before we go any further, let's test the Web API and verify that it creates the survey and saves it in the database. We'll use [Postman](https://www.getpostman.com/) to send a request to our API, so if you don't have that installed, go ahead and download and install that before proceeding.

1. Save all the changes you've made to the project files so far. Right-click the **SurveyService** project and choose **Set as Startup Project**.
1. Press **F5** to build and run the project. A browser should open and display a `403.14 Forbidden` error. This is normal. Copy the URL from the browser, which should look like `http://localhost:1266/`. (**Note:** The port number for your project may be different.)
1. Open Postman. Create a new tab if needed and configure the tab as follows:
    - Click the **GET** and change to **POST**.
    - In the text box labeled `Enter request URL` paste the URL you copied from the brower and add `api/surveys`
    - Click **Body** underneath the URL, then select the **raw** option.
    - Click **Text** and change to **JSON (application/json)**.
    - Enter the following in the text area below:
    
        ```json
        {
          "Survey": {
            "Name": "Test",
            "Duration": 5,
            "QuestionTitle": "What's your favorite color?",
            "QuestionChoices": "Red;Blue;Green"
          },
          "Sender": "adelev@contoso.com",
          "Recipients": [
            "benw@contoso.com",
            "alland@contoso.com"
          ]
        }
        ```

      The Postman window should look like this when you are done:

      ![The Postman request window configured to call the Web API](readme-images/postman-setup.PNG)
4. Click **Send** to call the API. If everything is working, you should get a response similar to the following:

    ```json
    {
      "Status": "Succeeded",
      "SurveyId": 1,
      "Expiration": "2017-03-17T19:55:47.2130827Z",
      "Participants": [
        {
          "Email": "benw@contoso.com",
          "LimitedToken": "Wf/uTh5vOClySZilp1O/s1HpZymeXz1YHfZ+esv2QQU="
        },
        {
          "Email": "alland@contoso.com",
          "LimitedToken": "LN9VDMD2CuPITjmomehTWXB0RHJ0Z0T5FPNCo1WCBbM="
        }
      ]
    }
    ```

Now you can verify the entries in the database using Visual Studio.

1. On the **View** menu, select **Server Explorer**.
1. Expand **Data Connections**, then double-click **SurveyContext**.
1. Right-click **SurveyContext** and choose **New Query**.
1. In the query window, enter the following query and click the **Execute** button.

    ```SQL
    SELECT * FROM SimpleSurveys
    ```
1. You should get back one result representing the data you sent with Postman.

### Update web app to call the Web API

Currently the web app uses a placeholder value for survey ID and user tokens when generating message cards. Now that the Web API can create and save surveys in the database, we can update the web app that generates actionable messages to call this API and use the generated values.

1. Use the NuGet Package Manager to install the **Microsoft.AspNet.WebApi.Client** package in the **SimpleSurvey** project.
1. Expand the **Controllers** folder in the **SimpleSurvey** project, and open the **SurveyController.cs** file.
1. Add the following `using` directives to the top of the file.

    ```C#
    using System.Net.Http;
    ```

1. Add the following method to the `SurveyController` class.

    ```C#
    private async Task<CreateSurveyResponse> CreateSurveyInService(Survey survey, string sender, string[] recipients)
    {
        CreateSurveyRequest request = new CreateSurveyRequest {
            Survey = survey,
            Sender = sender,
            Recipients = recipients
        };

        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("http://localhost:1266/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.PostAsJsonAsync("api/surveys", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<CreateSurveyResponse>();
        }
    }
    ```
1. Replace the `http://localhost:1266/` with the base URL of your web API. (This is the same value you copied from the browser in step 2 in the [Test creating a survey](#test-creating-a-survey) section).
1. Locate the `SendSurvey` method with the signature `public async Task<ActionResult> SendSurvey(Survey Survey, string ToRecipients)`. Replace that method with the following code.

    ```C#
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> SendSurvey(Survey Survey, string ToRecipients)
    {
        // Split the recipient list
        string[] recipients = ToRecipients.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);

        // Create a Graph client
        GraphServiceClient graphClient = new GraphServiceClient(
            new DelegateAuthenticationProvider(AddAccessToken));

        // Get the sender's email address
        Microsoft.Graph.User sender = await graphClient.Me.Request().GetAsync();
        string senderEmail = sender.Mail;

        var createSurveyResult = await CreateSurveyInService(Survey, senderEmail, recipients);

        // Send the survey
        string[] errors = await SendSurvey(
            createSurveyResult.SurveyId.ToString(), 
            Survey, 
            createSurveyResult.Participants,
            createSurveyResult.Expiration.ToString(), 
            graphClient);

        return Json(errors);
    }
    ```

1. Locate the `SendSurvey` method with the signature `private async Task<string[]> SendSurvey(string surveyId, Survey survey, Dictionary<string,string toRecipients, string closingTime, GraphServiceClient graphClient)` and update it to take a `List<SurveyParticipant>` parameter instead:
    1. Replace `Dictionary<string,string toRecipients` in the parameter list with `List<SurveyParticipant> toRecipients`.
    1. Change the line `foreach (KeyValuePair<string,string> recipient in toRecipients)` to `foreach (SurveyParticipant recipient in toRecipients)`.
    1. Change the line `string error = await SendSurvey(surveyId, survey, recipient.Key, recipient.Value, closingTime, graphClient);` to `string error = await SendSurvey(surveyId, survey, recipient, closingTime, graphClient);`

    The updated function should look like this:

    ```C#
    private async Task<string[]> SendSurvey(string surveyId, Survey survey, List<SurveyParticipant> toRecipients, string closingTime, GraphServiceClient graphClient)
    {
        List<string> errorMessages = new List<string>();

        foreach (SurveyParticipant recipient in toRecipients)
        {
            string error = await SendSurvey(surveyId, survey, recipient, closingTime, graphClient);
            if (!string.IsNullOrEmpty(error))
            {
                errorMessages.Add(error);
            }
        }

        return errorMessages.ToArray();
    }
    ```

1. Locate the `SendSurvey` method with the signature `private async Task<string> SendSurvey(string surveyId, Survey survey, string recipient, string token, string closingTime, GraphServiceClient graphClient)` and update it to take a `SurveyParticipant` parameter instead:
    1. Replace the `string recipient, string token,` parameters with `SurveyParticipant recipient`.
    1. Replace `recipient` with `recipient.Email` in the function.
    1. Replace `token` with `recipient.LimitedToken` in the function.

    The updated functdion should look like this:

    ```C#
    private async Task<string> SendSurvey(string surveyId, Survey survey, SurveyParticipant recipient, string closingTime, GraphServiceClient graphClient)
    {
        // Build up the card
        Card card = new Card();
        card.ThemeColor = "00B200";
        card.Title = survey.Name;
        card.Text = "Survey closes at **" + closingTime + "**";

        card.HideOriginalBody = false;
        card.Sections = new List<Section>();

        Section section = new Section();
        section.Title = survey.QuestionTitle;
        section.Actions = new List<MessageCard.Action>();

        ActionCard actionCard = new ActionCard();
        actionCard.Name = "Respond";
        actionCard.Inputs = new List<Input>();

        MultichoiceInput input = new MultichoiceInput();
        input.Id = "input";
        input.IsRequired = true;
        input.Title = "Select an option";
        input.Choices = new List<Choice>();

        string[] choices = survey.QuestionChoices.Split(';');
        for (int i = 0; i < choices.Length; i++)
        {
            input.Choices.Add(new Choice() { Display = choices[i], Value = (i + 1).ToString() });
        }

        actionCard.Inputs.Add(input);
        actionCard.Actions = new List<ExternalAction>()
        {
            // This HttpPOST action is defined so the following request is sent to the service:
            //
            // POST <service URL>
            // <Other HTTP headers>
            // ContentType: application/json
            //
            // {
            //      "UserId": "<id of user>",
            //      "SurveyId": "<id of the survey being responded to>",
            //      "Response": "{{input.value}}"
            // }
            new HttpPOST() {
                Name = "Submit",
                Target = "https://...", // TODO: Fix this with the real URL to web API
                Body = "{ \"UserId\": \"" + recipient.Email + "\", \"SurveyId\": \"" + surveyId + "\", \"LimitedToken\": \"" + recipient.LimitedToken + "\", \"Response\": \"{{input.value}}\" }",
                BodyContentType = "application/json"
            }
        };

        section.Actions.Add(actionCard);
        card.Sections.Add(section);

        Recipient toRecip = new Recipient()
        {
            EmailAddress = new EmailAddress() { Address = recipient.Email }
        };

        // Create the message
        Message actionableMessage = new Message()
        {
            Subject = "RESPONSE REQUESTED: " + survey.Name,
            ToRecipients = new List<Recipient>() { toRecip },
            Body = new ItemBody()
            {
                ContentType = BodyType.Html,
                Content = LoadSurveyMessageBody(card)
            }
        };

        try
        {
            await graphClient.Me.SendMail(actionableMessage, true).Request().PostAsync();
        } 
        catch (ServiceException graphEx)
        {
            return string.Format("Send to {0} failed - {1}: {2}", recipient, graphEx.Error.Code, graphEx.Error.Message);
        }

        return string.Empty;
    }
    ```

Now let's make a few minor changes to the message format to make it easier to see our changes.

1. In **SurveyController.cs**, locate the following line:

    ```C#
    card.HideOriginalBody = true;
    ```

    Change this value to `false`:

    ```C#
    card.HideOriginalBody = false;
    ```
1. In `LoadSurveyMessageBody`, replace the `Your email client cannot display this survey.` message with the JSON card payload. The updated method should look like this:

    ```C#
    private string LoadSurveyMessageBody(Card surveyCard)
    {
        return
            "<html>" +
            "  <head>" +
            "    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
            "    <script type=\"application/ld+json\">" +
            surveyCard.ToJson() +
            "    </script>" +
            "  <head>" +
            "  <body>" +
            surveyCard.ToJson() +
            "  </body>" +
            "</html>";
    }
    ```

With those changes, we're adding the JSON representation of the card into the message body and not hiding it. That way we can see all the details while we're testing.

### Test the web app

At this point we should be able to use the web app to create and send a survey.

1. Save all the changes you've made to the project files so far. Right-click the **SimpleSurvey** project and choose **Set as Startup Project**.
1. Press **F5** to build and run the project.

Sign in and send a survey to yourself. You should see the JSON card in the body of the message with the limited purpose token and the survey ID. However, if you try to respond to the survey, you should get a **The action could not be completed** error. That's because our action is still targeting `https://...`. In the next section we'll tackle implementing a target for our action.

---

## Implementing the action target

In this section we'll extend the Web API to include a new API that will serve as the target for the submit action in the actionable message.

### Implementing the RespondToSurvey action

Before we add a method to our Web API to handle the survey response, we need to address how the actionable message actions actually work. We're running the web app and web API locally using IIS Express and Visual Studio, which won't work for an actionable message! The target of any action on an actionable message has to be a public HTTPS endpoint. While we can do HTTPS via Visual Studio, the endpoint is `http://localhost`, which the Office 365 servers can't reach.

We could solve this by publishing our project to a public server like an Azure website. However, another option that makes for easy testing on our local devlopment machines is [ngrok](https://ngrok.com/). We'll use that for this guide.

#### Setup ngrok

Head over to the [ngrok website](https://ngrok.com/) and download the tool. I recommend saving it somewhere in your PATH so it's easier to use from the command line.

1. In Visual Studio, select the **SurveyService** project in **Solution Explorer**. In the **Properties** window, locate the **URL** value and copy the port number from the URL.

    ![The port number in the URL property](readme-images/port-number.PNG)
1. Open a command prompt and run ngrok using the following command. Replace the `<port-number>` placeholder with the port number you copied in the previous step.

    ```Shell
    ngrok http <port-number> -host-header=localhost:<port-number>
    ```
1. In the ngrok output, locate the forwarding HTTPS URL and copy it. This will be our target URL for actions.

    ![The ngrok output with the HTTPS forwarding URL](readme-images/ngrok.PNG)

Leave the command prompt with ngrok running open. As long as ngrok is running, any requests made to the forwarding URLs will be tunneled to localhost.

> **Note:** If you stop ngrok (with CTRL+C), you can always run it again. However, the URL will change, so keep in mind that you will need to update your code each time.

#### Create a model for survey responses

1. Right-click the **SurveyModels** project and choose **Add**, then **Class**. Name the class `RespondToSurveyRequest`.
1. Open **RespondToSurveyRequest.cs** and update the class with the following code.

    ```C#
    public class RespondToSurveyRequest
    {
        public string UserId { get; set; }
        public int SurveyId { get; set; }
        public string LimitedToken { get; set; }
        public string Response { get; set; }
    }
    ```

Notice that this matches the JSON structure of the `body` value in the action in our card.

#### Add a controller for responses

1. Right-click the **Controllers** folder under the **SurveyService** project and choose **Add**, then **Controller**. Choose **Web API 2 Controller - Empty** and click **Add**. Name the controller **ResponsesController** and click **Add**.
1. Open the **ResponsesController.cs** file and add the following `using` directives at the top of the file.

    ```C#
    using SurveyModels;
    using SurveyService.Models;
    using SurveyService.Tokens;
    ```
1. Add the following method to the `ResponsesController` class.

    ```C#
    public IHttpActionResult PostResponse(RespondToSurveyRequest surveyResponse)
    {
        return Ok(surveyResponse);
    }
    ```

That doesn't do much yet, but it's enough for us to verify that the ngrok proxy is working. Let's update the web app to set a valid target URL on the actions so we can test.

#### Update the web app

1. Open the **SurveyController.cs** file in the **SimpleSurvey** project and add the following member to the `SurveyController` class:

    ```C#
    // TODO: Update this value with the new ngrok forwarding URL 
    // each time you restart ngrok
    private string actionBaseUrl = "https://d8765f20.ngrok.io";
    ```

1. If ngrok is not running, go ahead and start it and copy the HTTPS fowarding URL. Replace the value with your current URL.
1. Find the line that reads:

    ```C#
    Target = "https://...",
    ```

    And update it like so:

    ```C#
    Target = actionBaseUrl + "/api/responses",
    ```

#### Test the response API

1. Save all the changes you've made to the project files so far. Right-click the **SurveyService** project and choose **Set as Startup Project**.
1. Set a breakpoint on the `PostResponse` method on the `ResponsesController` class.
1. Press **F5** to build and run the project.
1. Browse to the Simple Survey web app, sign in, and send a survey to yourself.
1. Using Outlook on the web, check the received message. In the body of the message, you should see something similar to:

    ```json
    "target":"https:\/\/d8765f20.ngrok.io\/api\/responses"
    ```
1. Choose an option on the survey and click the **Submit** button.

If everything is working correctly, Visual Studio should break on your breakpoint. If you press **F5** to continue, you should see a message that the action completed successfully in Outlook on the web.

#### Recording the response

Now let's modify the `PostResponse` method to actually record the response. Replace the existing `PostResponse` with the following.

```C#
public IHttpActionResult PostResponse(RespondToSurveyRequest surveyResponse)
{
    try
    {
        using (var db = new SurveyContext())
        {
            // Make sure the survey ID is valid
            SimpleSurvey survey = db.Surveys.Find(surveyResponse.SurveyId);
            if (survey == null)
            {
                return BadRequest("Invalid survey ID");
            }

            // Is the response a valid one?
            string[] validResponses = survey.QuestionChoices.Split(';');
            int userResponseIndex = Convert.ToInt32(surveyResponse.Response) - 1;
            if (userResponseIndex < 0 || userResponseIndex >= validResponses.Length)
            {
                return BadRequest("Invalid response");
            }

            // Is the user a participant?
            Participant responder = survey.Participants.FirstOrDefault(p => p.Email == surveyResponse.UserId.ToLower());
            if (responder == null)
            {
                return BadRequest("Invalid participant");
            }

            // Is the limited purpose token a match?
            string expectedToken = TokenGenerator.GenerateLimitedToken(survey.SimpleSurveyId, responder.Email, responder.TokenSalt);
            if (surveyResponse.LimitedToken != expectedToken)
            {
                return BadRequest("Limited purpose token validation failed");
            }

            // Has the user already responded?
            Response previousResponse = survey.Responses.FirstOrDefault(r => r.Participant == responder);
            if (previousResponse != null)
            {
                return BadRequest("Participant has already responded");
            }

            // Has the survey expired?
            if (DateTime.Compare(survey.Expiration, DateTime.UtcNow) < 0)
            {
                return BadRequest("Survey expired");
            }

            // Create the response object
            Response response = new Response
            {
                Participant = responder,
                ParticipantResponse = validResponses[userResponseIndex],
                Survey = survey
            };

            db.Responses.Add(response);
            db.SaveChanges();

            return Ok();
        }
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

This version of the method checks a number of conditions before writing to the database.

- Is the survey ID valid?
- Is the user a participant for the survey?
- Does the limited purpose token validate?
- Has the user already responded?
- Has the survey expired?

If everything checks out, only then does it write to the database. Go ahead and try this out now. You should be able to respond to a survey using the actionable message. Now try responding a second time, you should get a **The action could not be completed** this time.

> **Note:** Because this demo solution is not registered with Microsoft, you cannot send actionable messages to anyone outside of your organization. Also, while you can send the message to others in your organization, and they will see the message card, they cannot invoke the actions.

This covers the basic functionality, but it isn't quite a complete solution.

- The `PostResponse` method is open. If an attacker obtained a limited purpose token for a user, they could post a response.
- When the post fails, the user is given no meaningful error message, just **The action could not be completed**.
- When the post succeeds, the user gets a generic success message rather than a useful response.

Let's fix these issues!

#### Preventing posts from outside sources

Our `PostResponse` API should only ever except POST requests from the Office 365 service. To help our service determine that the POST is legitimate, Office 365 adds a bearer token to each request in the `Authorization` header. So all we need to do is validate it. Microsoft has made it easy for us to do that by providing a NuGet package that will do all the validation for us.

1. On the **Tools** menu, choose **NuGet Package Manager**, then **Manage NuGet Packages for Solution...**. Click the **Browse** tab, then enable the **Include prerelease** option. Search for `Microsoft.O365.ActionableMessages.Utilities`. Select **Microsoft.O365.ActionableMessages.Utilities** in the list of packages, then put a check in the box next to the **SurveyService** project. Click **Install**.
1. Open the **ResponsesController.cs** file and add the following `using` directive at the top of the file.

    ```C#
    using Microsoft.Outlook.ActionableMessages.Authentication;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    ```
1. Add the following member to the `ResponsesController` class (replace the value with your current ngrok forwarding URL):

    ```C#
    // TODO: Update this value with the new ngrok forwarding URL 
    // each time you restart ngrok
    private string actionBaseUrl = "https://215fe7e9.ngrok.io";
    ```
1. Add a new function to the `ResponsesController` class.

    ```C#
    private async Task<bool> ValidateAuthorizationHeader(AuthenticationHeaderValue authHeader, string targetUrl, string userId)
    {
        // Validate that we have a bearer token
        if (authHeader == null ||
            !string.Equals(authHeader.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(authHeader.Parameter))
        {
            return false;
        }

        // Validate the token
        ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
        ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(authHeader.Parameter, targetUrl);
        if (!result.ValidationSucceeded)
        {
            return false;
        }

        // Token is valid, now check the sender and action performer
        // Both should equal the user
        if (!string.Equals(result.ActionPerformer, userId, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(result.Sender, userId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
    ```
1. Update the `PostResponse` function to be asynchronous. Change the function declaration:

    ```C#
    public IHttpActionResult PostResponse(RespondToSurveyRequest surveyResponse)
    ```

    to the following:

    ```C#
    public async Task<IHttpActionResult> PostResponse(RespondToSurveyRequest surveyResponse)
    ```

1. Update the function to call the `ValidateAuthorizationHeader` function. Insert the following code before the `try` statement:

    ```C#
    // Validate the authorization header
    bool isTokenValid = await ValidateAuthorizationHeader(Request.Headers.Authorization,
        actionBaseUrl, surveyResponse.UserId);
    if (!isTokenValid)
    {
        return Unauthorized();
    }
    ```

You should now be able to run and test this new code. When posting a response from Outlook on the web, you should see no change. To validate that it actually blocks POST requests that don't have a valid bearer token, try using Postman to POST to `<your ngrok URL>/api/responses` with a simple response payload:

```json
{
    "UserId": "adelev@contoso.com", 
    "SurveyId": "5",
    "LimitedToken": "yxMhqvjGMoKu2yHxD4fEjBx45MNbXA\/Gd+xvOywT8Kg=",
    "Response": "Hello" 
}
```

You should get a `401 Unauthorized` response. Now let's move on to improving error messages.

#### Return a custom error response

For some of our error cases, we're not concerned about providing a friendly message. For example, if the bearer token is invalid, we shouldn't be concerned about giving an unauthorized user a helpful response! However, in some cases a legitimate user may do something wrong, and we should definitely be helpful in those cases. For example, when we tried responding to a survey more than once, we only got a generic message. Let's update the service to explain to the user what exactly went wrong.

In order to return a custom message, we need to include a [CARD-ACTION-STATUS](https://dev.outlook.com/actions/reference#reporting-an-actions-execution-success-or-failure) header in the response.

1. Add the following function to the `ResponsesController` class to generate a 401 error with a `CARD-ACTION-STATUS` message.

    ```C#
    private IHttpActionResult GenerateFriendlyResponse(string message)
    {
        HttpResponseMessage friendlyResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        friendlyResponse.Headers.Add("CARD-ACTION-STATUS", message);
        return ResponseMessage(friendlyResponse);
    }
    ```
1. Update the `PostResponse` function to call this method rather than `BadRequest` when the user has already responded.

    ```C#
    // Has the user already responded?
    Response previousResponse = survey.Responses.FirstOrDefault(r => r.Participant == responder);
    if (previousResponse != null)
    {
        return GenerateFriendlyResponse("You've already responded to this survey!");
    }
    ```
1. Update the `PostResponse` function to call this method rather than `BadRequest` when the survey has expired.

    ```C#
    // Has the survey expired?
    if (DateTime.Compare(survey.Expiration, DateTime.UtcNow) < 0)
    {
        return GenerateFriendlyResponse("This survey's response window has closed! Your response has not been recorded.");
    }
    ```

Leave the other validation checks as they are. Those would most likely only happen if someone is trying to spoof a real response, so we'll leave them alone.

Go ahead and test your service now. Try responding to a survey twice. The second time you should get a message **You've already responded to this survey!**.

Now let's look at improving the user experience when they successfully respond.

#### Return a refresh card

Now that we know how to return custom responses, we could use the same approach to send a message like "Thanks for responding!" to the user. However, that isn't a great solution since they still have the choice dropdown and the submit button on the card. There's nothing to stop the user from trying to respond again. It would be better if we could remove that UI altogether. With [refresh cards](https://dev.outlook.com/actions/reference#refresh-cards), we can!

The idea with refresh cards is that our service can return a whole new JSON card payload, and the client will replace what's there with the new card. This allows us to change the UI to remove the actions they can no longer take, and to provide more information. For example, after the user responds, we could return a snapshot of the current responses. Let's update the service to do just that.

1. Add a function to the `SimpleSurvey` class to generate a statistics card containing the total number of responses currently in the database. Open the **SimpleSurvey.cs** file in the **Models** folder and add `using MessageCard;` to the top of the file. Then add the following code:

    ```C#
    public Card GenerateSurveyResultSnapshotCard()
    {
        Card statsCard = new Card();
        statsCard.ThemeColor = "00B200";
        statsCard.HideOriginalBody = true;
        statsCard.Title = $"Thanks for responding to \"{Name}\"";

        Section section = new Section();
        section.Title = $"The question was: \"{QuestionTitle}\"";
        section.Text = $"Responses as of {DateTime.Now.ToString("MM/dd/yy \"at\" HH:mm \"UTC\"")}";

        List<Fact> facts = new List<Fact>();

        string[] surveyChoices = QuestionChoices.Split(';');
        foreach (string choice in surveyChoices)
        {
            int numResponses = Responses.FindAll(r => string.Equals(r.ParticipantResponse, choice, StringComparison.OrdinalIgnoreCase)).Count;
            facts.Add(new Fact() { Name = choice, Value = numResponses.ToString() });
        }

        section.Facts = facts;

        statsCard.Sections = new List<Section>() { section };

        return statsCard;
    }
    ```

    This builds a card that utilizes the `facts` field of a [section](https://dev.outlook.com/actions/reference#section-fields). Facts are used to render a list of key/value pairs, which is perfect for a list of responses and counts. It queries the database to get the current counts for each possible response.

1. Add a method to generate a `200` response with the card's JSON in the body and the `CARD-UPDATE-IN-BODY` header set to `true`.

    ```C#
    private IHttpActionResult GenerateRefreshCardResponse(Card refreshCard)
    {
        HttpResponseMessage refreshCardResponse = new HttpResponseMessage(HttpStatusCode.OK);
        refreshCardResponse.Headers.Add("CARD-UPDATE-IN-BODY", "true");

        // Serialize the card as JSON to the response body
        refreshCardResponse.Content = new StringContent(refreshCard.ToJson(), System.Text.Encoding.UTF8, "application/json");
        return ResponseMessage(refreshCardResponse);
    }
    ```

1. Update the `PostResponse` function to generate a refresh card and send it in the response. Replace the `return Ok();` line with the following.

    ```C#
    return GenerateRefreshCardResponse(survey.GenerateSurveyResultSnapshotCard());
    ```

Save all of your changes and test. Send a new survey using the web app and then use Outlook web app to respond. You should now see that the card is refreshed with a count of the current responses.

![An example of the refresh card generated by the service](readme-images/refresh-card.PNG)

Now that we have the survey response feature working, let's look at reporting the results once the survey closes.

---

## Sending the results to an Office 365 Group

Now let's look at what happens when a survey closes. The whole point of doing a survey is to collect data, so there has to be some way for the sender to view the results. We could enhance the web app to allow the user to view their survey results, but that requires the sender to remember to go log in to the site and view the results at the right time. Instead, we'll track the closing time in our web API and send out a message card with the results. We could certainly send this directly to the sender, using the same methods we use in the web app to send the survey. But since we've already covered that, let's look at an alternative: [Connectors for Groups](https://dev.outlook.com/Connectors). We can use this to push a message card directly to a group, so all members of that group can see the results.

### Set up a group

First we need to choose a group to receive the results. You can either use an existing group or create a new one.

1. Login to Outlook on the web, and go to the **People** app.

    ![The People app in the Office menu](readme-images/people-app.PNG)
1. Expand **Groups** in the left-hand menu, then either select an existing group or create a new one.
1. With the group selected, select **Connectors** in the top menu.

    ![The top menu of a group with Connectors highlighted](readme-images/connectors-menu-item.PNG)
1. In the list of available connectors, select **Add** for the **Incoming Webhook** connector.
1. Enter a name for the connector and select **Create**.
1. Copy the URL that is generated and save it, we'll need it soon.

    ![The generated URL for an incoming webhook](readme-images/webhook-url.PNG)

### Generate a final results card

Now let's add a function to the `SimpleSurvey` class to generate a final results card. This will be very similar to the snapshot card we used as a refresh card, but with slight differences.

1. Open the **SimpleSurvey.cs** file in the **Models** folder and add the following code:

    ```C#
    public Card GenerateSurveyResultFinalCard()
    {
        Card statsCard = new Card();
        statsCard.ThemeColor = "00B200";
        statsCard.HideOriginalBody = true;
        statsCard.Title = $"Final result for \"{Name}\"";
        statsCard.Text = $"{Responses.Count} of {Participants.Count} participants responded.";

        Section section = new Section();
        section.Title = $"The question was: \"{QuestionTitle}\"";
        section.Text = "Here's how particpants responded";

        List<Fact> facts = new List<Fact>();

        string[] surveyChoices = QuestionChoices.Split(';');
        foreach (string choice in surveyChoices)
        {
            int numResponses = Responses.FindAll(r => string.Equals(r.ParticipantResponse, choice, StringComparison.OrdinalIgnoreCase)).Count;
            facts.Add(new Fact() { Name = choice, Value = numResponses.ToString() });
        }

        section.Facts = facts;

        section.Actions = new List<MessageCard.Action>()
        {
            // Add a button to open a URL
            // For example, this could be a link into a web app that displays the
            // results in a rich manner 
            new OpenUri()
            {
                Name = "View Result Details",
                Targets = new List<OpenUriTarget>()
                {
                    new OpenUriTarget() { OS = "default", Uri = "https://..." }
                }
            }
        };

        statsCard.Sections = new List<Section>() { section };

        return statsCard;
    }
    ```

A big difference between the snapshot and the final card is that we add an OpenUri action. We're just adding a placeholder URL here, but the idea is that in a real survey application, you may have a web app view of the results that gives more details or a richer view of the results.

### Create an API for sending results

Now let's create a new API to query the database for all expired surveys and send their results.

1. Right-click the **Controllers** folder in the **SurveyService** project and choose **Add**, then **Controller**. Choose **Web API 2 Controller - Empty** and click **Add**. Name the controller **SendResultsController** and click **Add**.
1. Open the **SendResultsController.cs** file and add the following `using` directives at the top of the file.

    ```C#
    using SurveyService.Models;
    using MessageCard;
    using System.Threading.Tasks;
    ```
1. Add a private string member to the `SendResultsController` class for the webhook URL you generated earlier. Replace the value in the code below with the value you copied.

    ```C#
    private string webHookUrl = "https://outlook.office.com/webhook/...";
    ```
1. Add a method to the `SendResultsController` class to send results.

    ```C#
    public async Task<IHttpActionResult> PostSendResults()
    {
        try
        {
            using (var db = new SurveyContext())
            {
                var expiredSurveys = db.Surveys.Where(s => !s.ResultsReported && DateTime.Compare(s.Expiration, DateTime.UtcNow) < 0).ToList();

                if (expiredSurveys.Count > 0)
                {
                    using (var client = new HttpClient())
                    {
                        foreach (SimpleSurvey survey in expiredSurveys)
                        {
                            // Generate the card
                            Card resultsCard = survey.GenerateSurveyResultFinalCard();

                            // Post the card to the webhook
                            HttpResponseMessage response = await client.PostAsJsonAsync(webHookUrl, resultsCard);
                            response.EnsureSuccessStatusCode();

                            // Mark the survey as reported
                            survey.ResultsReported = true;
                            db.SaveChanges();
                        }
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    ```

### Testing sending results

Now that we have the API in place to send out the results, we could invoke it in a number of different ways. We could have a scheduled task or a Windows service that calls the API at set intervals. But for our purposes, we'll just invoke it manually using Postman.

1. Open Postman. Create a new tab if needed and configure the tab as follows:
    - Click the **GET** and change to **POST**.
    - In the text box labeled `Enter request URL` paste the base URL for your Web API add `api/sendresults`
1. Click **Send**.

Assuming everything worked, and that you have surveys that have expired in your database, you should get result messages posted into your Office 365 Group. 

---

## Wrapping Up

Let's sum up what the exercises in this lab have covered.

- We saw how to send actionable messages directly to users in email. In this solution we used the Microsoft Graph, but you could also use SMTP.
- We saw how to implement a Web API to act as the target for the actions in the actionable message. In this solution we used an ASP.NET Web API, but you could use any web server technology.
- We saw how to validate the bearer token on incoming action requests.
- We saw how to return custom error responses and refresh cards in response to actions.
- We saw how to post actionable messages directly to an Office 365 group.

We hope that gives you a taste of what you can accomplish with actionable messages and connectors. To learn more, visit [https://dev.outlook.com/Actions](https://dev.outlook.com/Actions) and [https://dev.outlook.com/Connectors](https://dev.outlook.com/Connectors).