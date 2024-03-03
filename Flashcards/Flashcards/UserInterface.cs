using System;
using Spectre.Console;
using static Flashcards.Enums;

namespace Flashcards
{
	internal class UserInterface
	{
		internal static void MainMenu()
		{
			var isMenuRunning = true;
			while (isMenuRunning)
			{
				var usersChoice = AnsiConsole.Prompt(
					new SelectionPrompt<MainMenuChoices>()
					.Title("what would u like to do?")
					.AddChoices(
						MainMenuChoices.ManageFlashcards,
						MainMenuChoices.ManageStacks,
                        MainMenuChoices.StudySession, //new
                        MainMenuChoices.StudyHistory, //new
                        MainMenuChoices.Quit
					));
				switch (usersChoice)
				{
					case MainMenuChoices.ManageFlashcards:
                        FlashcardsMenu();
                        break;
                    case MainMenuChoices.ManageStacks:
                        StacksMenu();
                        break;
                    case MainMenuChoices.StudySession: //new
                        StudySession();
                        break;
                    case MainMenuChoices.StudyHistory:
                        ViewStudyHistory();
                        break;
                    case MainMenuChoices.Quit:  
                        System.Console.WriteLine("Goodbye");
                        isMenuRunning = false;
                        break;
                }
			}
		}

		internal static void StacksMenu()
		{
			var isMenuRunning = true;
			while (isMenuRunning)
			{
                var usersChoice = AnsiConsole.Prompt(
					new SelectionPrompt<StackChoices>()
					.Title("what would u like to do?")
					.AddChoices(
                        StackChoices.AddStack,
                        StackChoices.DeleteStack,
                        StackChoices.UpdateStack,
                        StackChoices.ViewStacks,
                        StackChoices.ReturnToMainMenu
                    ));
				switch (usersChoice)
				{
					case StackChoices.AddStack:
						AddStack();
						break;
					case StackChoices.DeleteStack:
						DeleteStack();
						break;
					case StackChoices.UpdateStack:
						UpdateStack();
						break;
					case StackChoices.ViewStacks:
						ViewStacks();
						break;
					case StackChoices.ReturnToMainMenu:
						isMenuRunning = false;
						break;
                }
            }
		}

		private static void AddStack()
		{
            Stack stack = new();
            stack.Name = AnsiConsole.Ask<string>("Insert stack name.");
            while(string.IsNullOrEmpty(stack.Name))
            {
                stack.Name = AnsiConsole.Ask<string>("Insert stack name that is not empty.");
            }

            var dataAccess = new DataAccess();
            dataAccess.InsertStack(stack);
		}
        private static int ChooseStack(string message)
        {
            var dataAccess = new DataAccess();
            var stacks = dataAccess.GetAllStacks();
            var stacksArray = stacks.Select(item => item.Name).ToArray();
            var option = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(message)
            .AddChoices(stacksArray));

            var stackId = stacks.Single(item => item.Name == option).Id;
            return stackId;
        }

        private static int ChooseFlashcard(string message, int stackId)
        {
            var dataAccess = new DataAccess();
            var flashcards = dataAccess.GetFlashcards(stackId);
            int length = flashcards.Count();
            Console.WriteLine($"The amount of returned flashcards is {length}");
            var flashcardsArray = flashcards.Select(item => item.Question).ToArray();
            var option = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(message)
            .AddChoices(flashcardsArray));
            var flashcardId = flashcards.Single(item => item.Question == option).Id;
            return flashcardId;
        }

		private static void UpdateStack()
		{
            var stack = new Stack();
            stack.Id = ChooseStack("Choose a stack: ");
            stack.Name = AnsiConsole.Ask<string>("Insert Stack's Name.");
            var dataAccess = new DataAccess();
            dataAccess.UpdateStack(stack);
        }

		private static void DeleteStack()
		{
            int id = ChooseStack("Choose a stack");
            var dataAccess = new DataAccess();
            dataAccess.DeleteStack(id);
		}

		private static void ViewStacks()
		{
            var dataAccess = new DataAccess();
            var stacks = dataAccess.GetAllStacks();
            var table = new Table();
            table.AddColumn("stack id");
            table.AddColumn("stack name");
            foreach (var stack in stacks)
            {
                table.AddRow(stack.Id.ToString(), stack.Name);
            }
            AnsiConsole.Write(table);
        }

        internal static void FlashcardsMenu()
        {
            var isMenuRunning = true;

            while (isMenuRunning)
            {
                var usersChoice = AnsiConsole.Prompt(
                       new SelectionPrompt<FlashcardChoices>()
                        .Title("What would you like to do?")
                        .AddChoices(
                           FlashcardChoices.ViewFlashcards,
                           FlashcardChoices.AddFlashcard,
                           FlashcardChoices.UpdateFlashcard,
                           FlashcardChoices.DeleteFlashcard,
                           FlashcardChoices.ReturnToMainMenu)
                        );

                switch (usersChoice)
                {
                    case FlashcardChoices.ViewFlashcards:
                        ViewFlashcards();
                        break;
                    case FlashcardChoices.AddFlashcard:
                        AddFlashcard();
                        break;
                    case FlashcardChoices.DeleteFlashcard:
                        DeleteFlashcard();
                        break;
                    case FlashcardChoices.UpdateFlashcard:
                        UpdateFlashcard();
                        break;
                    case FlashcardChoices.ReturnToMainMenu:
                        isMenuRunning = false;
                        break;
                }
            }
        }

        private static void UpdateFlashcard()
        {
            var stackId = ChooseStack("Choose a stack:");
            var flashcardId = ChooseFlashcard("Choose a flashcard: ", stackId);

            var properties = new Dictionary<string, object>();
            if (AnsiConsole.Confirm("would u like to update question?"))
            {
                var question = GetQuestion();
                properties.Add("Question", question);
            }

            if (AnsiConsole.Confirm("would u like to update answer?"))
            {
                var answer = GetAnswer();
                properties.Add("Answer", answer);
            }

            if (AnsiConsole.Confirm("do u want to update stack?"))
            {
                var stack = ChooseStack("Choose a stack for this flashcard:");
                properties.Add("StackId", stack);
            }

            var dataAccess = new DataAccess();
            dataAccess.UpdateFlashcard(flashcardId, properties);
        }

        private static void DeleteFlashcard()
        {
            var stackId = ChooseStack("Choose a stack to delete from");
            Console.WriteLine($"stack id is: {stackId}");
            var flashcardId = ChooseFlashcard("Choose a flashcard to delete", stackId);
            var dataAccess = new DataAccess();
            dataAccess.DeleteFlashcard(flashcardId);
        }

        private static void AddFlashcard()
        {
            Flashcard flashcard = new();

            flashcard.StackId = ChooseStack("Choose a stack for the new flashcard");
            flashcard.Question = GetQuestion();
            flashcard.Answer = GetAnswer();

            var dataAccess = new DataAccess();
            dataAccess.InsertFlashcard(flashcard);
        }

        private static void ViewFlashcards()
        {
            var dataAccess = new DataAccess();
            var stackId = ChooseStack("Choose a stack u want to use flashcards from");
            var flashcards = dataAccess.GetFlashcards(stackId);
            var table = new Table();
            table.AddColumn("flashcard id");
            table.AddColumn("question");
            table.AddColumn("answer");
            table.AddColumn("stack id");
            foreach (var flashcard in flashcards)
            {
                table.AddRow(flashcard.Id.ToString(), flashcard.Question, flashcard.Answer, flashcard.StackId.ToString());
            }
            AnsiConsole.Write(table);
        }

        private static string GetQuestion()
        {
            var question = AnsiConsole.Ask<string>("Insert Question.");

            while (string.IsNullOrEmpty(question))
            {
                question = AnsiConsole.Ask<string>("Question can't be empty. Try again.");
            }

            return question;
        }

        private static string GetAnswer()
        {
            var answer = AnsiConsole.Ask<string>("Insert answer.");

            while (string.IsNullOrEmpty(answer))
            {
                answer = AnsiConsole.Ask<string>("Answer can't be empty. Try again.");
            }

            return answer;
        }

        private static void StudySession()
        {
            var id = ChooseStack("Choose a stack u want to study");
            var dataAccess = new DataAccess();
            var flashcards = dataAccess.GetFlashcards(id);

            var session = new StudySession();
            session.Questions = flashcards.Count();
            session.StackId = id;
            session.Date = DateTime.Now;

            var correctAnswers = 0; // tracking the results

            foreach (var flashcard in flashcards)
            {
                var answer = AnsiConsole.Ask<string>($"{flashcard.Question}: ");

                // We're only checking if the answer is empty. 
                while (string.IsNullOrEmpty(answer))
                    answer = AnsiConsole.Ask<string>($"Answer can't be empty. {flashcard.Question}: ");

                // this ignores leading and trailing whitespaces and the case of the characters
                if (string.Equals(answer.Trim(), flashcard.Answer, StringComparison.OrdinalIgnoreCase))
                {
                    correctAnswers++;
                    Console.WriteLine($"Correct!\n"); // \n adds a new line for better readability
                }
                else
                {
                    Console.WriteLine($"Wrong, the answer is {flashcard.Answer}\n");
                }

            }
            Console.WriteLine($"You've got {correctAnswers} out of {flashcards.Count()}!");
            session.Time = DateTime.Now - session.Date;
            dataAccess.InsertStudySession(session);
        }

        internal static void ViewStudyHistory()
        {
            var dataAccess = new DataAccess();
            var sessions = dataAccess.GetStudySessionData();
            var table = new Table();
            table.AddColumn("Date");
            table.AddColumn("Stack");
            table.AddColumn("Result");
            table.AddColumn("Percentage");
            table.AddColumn("Duration");

            foreach (var session in sessions)
            {
                table.AddRow(session.Date.ToShortDateString(), session.StackName,
                    $"{session.CorrectAnswers} out of {session.Questions}", $"{session.Percentage}%", session.Time.ToString());
            }
            AnsiConsole.Write(table);
        }
    }
}

