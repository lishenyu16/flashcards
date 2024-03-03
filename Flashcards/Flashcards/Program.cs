using Flashcards;

var dataAccess = new DataAccess();

dataAccess.CreateTables();
Seed.SeedRecords();

UserInterface.MainMenu();