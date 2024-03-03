using System;
namespace Flashcards
{
	internal class Enums
	{
		internal enum MainMenuChoices
		{
            ManageStacks,
            ManageFlashcards,
            StudySession,
            StudyHistory,
            Quit
        }

        internal enum StackChoices
        {
            ViewStacks,
            AddStack,
            DeleteStack,
            UpdateStack,
            ReturnToMainMenu
        }

        internal enum FlashcardChoices
        {
            ViewFlashcards,
            AddFlashcard,
            DeleteFlashcard,
            UpdateFlashcard,
            ReturnToMainMenu
        }
	}
}

