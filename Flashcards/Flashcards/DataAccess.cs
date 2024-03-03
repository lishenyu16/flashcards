using System.Transactions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Flashcards.Models.DTO;

namespace Flashcards
{
    public class DataAccess
	{
		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();
		private string ConnectionString;
		public DataAccess()
		{
			ConnectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
		}

		internal void DeleteTables()
		{
			try
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
                    connection.Open();
                    string sqlDropStudySessions = @"Drop table studySessions";
                    connection.Execute(sqlDropStudySessions);
                    string sqlDropFlashcards = @"Drop table flashcards";
					connection.Execute(sqlDropFlashcards);
					string sqlDropStacks = @"Drop table stacks";
					connection.Execute(sqlDropStacks);
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine($"there was a problem for drpping the tables: {ex.Message}");
			}
		}

		internal void CreateTables()
		{
			try
			{
				using (var conn = new SqlConnection(ConnectionString))
				{
					conn.Open();
					string createTableSql = @"IF NOT exists (SELECT * FROM sys.tables WHERE name = 'Stacks')
					CREATE TABLE STACKS (
						Id int IDENTITY(1,1) NOT NULL,
                        Name NVARCHAR(30) NOT NULL UNIQUE,
                        PRIMARY KEY (Id)
					)";
					conn.Execute(createTableSql);

					string createFlashcardsTableSql = @"
					IF NOT EXISTS (SELECT * FROM SYS.TABLES WHERE NAME = 'Flashcards')
					CREATE TABLE FLASHCARDS (
						Id int Identity (1,1) not null primary key,
						Question NVARCHAR(30) NOT NULL,
						Answer NVARCHAR(30) NOT NULL,
						StackId int NOT NULL 
                            FOREIGN KEY 
                            REFERENCES Stacks(Id) 
                            ON DELETE CASCADE 
                            ON UPDATE CASCADE
					)";
					conn.Execute(createFlashcardsTableSql);

                    string createStudySessionTableSql = @"
					IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudySessions')
					CREATE TABLE StudySessions (
						Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
						Questions int NOT NULL,
						Date DateTime NOT NULL, 
						CorrectAnswers int NOT NULL,
						Percentage AS (CorrectAnswers * 100) / Questions PERSISTED,
						Time TIME NOT NULL,
						StackId int NOT NULL
							FOREIGN KEY 
							REFERENCES Stacks(Id)
							ON DELETE CASCADE 
							ON UPDATE CASCADE
					 );";
                    conn.Execute(createStudySessionTableSql);

                }
			}
			catch (Exception ex)
			{
                Console.WriteLine($"There was a problem creating the tables: {ex.Message}");
            }
		}

		internal IEnumerable<Stack> GetAllStacks()
		{
			try
			{
                using (var conn = new SqlConnection(ConnectionString))
				{
					conn.Open();
					string query = "Select * from stacks";
                    var records = conn.Query<Stack>(query);
                    return records;
				}
			}
			catch (Exception ex)
			{
                Console.WriteLine($"There was a problem retrieving stacks: {ex.Message}");
                return new List<Stack>();
            }
		}

		internal void UpdateStack(Stack stack)
		{
			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				string sql = @"update stacks set name = @Name where id = @Id";
				connection.Execute(sql, new { stack.Name, stack.Id });
			}
		}

		internal void InsertStack(Stack stack)
		{
			try
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					string insertSql = @"insert into stacks(name) values(@Name)";
					connection.Execute(insertSql, new { stack.Name });
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		internal void InsertFlashcard(Flashcard flashcard)
		{
			try
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					string sql = @"insert into flashcards(question, answer,stackid) values(@Question, @Answer, @StackId)";

					connection.Execute(sql, new { flashcard.Question, flashcard.Answer, flashcard.StackId });
				}
			}
			catch(Exception ex)
			{

			}
		}

        internal void UpdateFlashcard(int flashcardId, Dictionary<string, object> propertiesToUpdate)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string updateQuery = "UPDATE flashcards SET ";
                var parameters = new DynamicParameters();

                foreach (var kvp in propertiesToUpdate)
                {
                    updateQuery += $"{kvp.Key} = @{kvp.Key}, ";
                    parameters.Add(kvp.Key, kvp.Value);
                }

                updateQuery = updateQuery.TrimEnd(',', ' ');

                updateQuery += " WHERE Id = @Id";
                parameters.Add("Id", flashcardId);

                connection.Execute(updateQuery, parameters);
            }
        }

        internal void BulkInsertRecords(List<Stack> stacks, List<Flashcard> flashcards)
		{
			SqlTransaction sqlTransaction = null;
			try
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					sqlTransaction = connection.BeginTransaction();
					connection.Execute("insert into stacks(name) values(@name)", stacks, transaction: sqlTransaction);
					connection.Execute("insert into flashcards(Question, Answer, StackId) values(@Question, @Answer, @StackId)", flashcards, transaction: sqlTransaction);

					sqlTransaction.Commit();
				}
			}
			catch(Exception ex)
			{
                Console.WriteLine($"There was a problem bulk inserting records: {ex.Message}");

                if (sqlTransaction != null)
                {
                    sqlTransaction.Rollback(); // Rollback the transaction if an exception occurs
                }
            }
		}

		internal void DeleteStack(int id)
		{
			try
			{
				using(var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					string sql = "delete from stacks where id = @id";
					connection.Execute(sql, new { id });
				}
			}
			catch(Exception ex)
			{

			}

		}

		internal void DeleteFlashcard(int id)
		{
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM stack WHERE Id = @Id";

                    int rowsAffected = connection.Execute(deleteQuery, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was a problem deleting the stack: {ex.Message}");

            }
        }

		internal IEnumerable<Flashcard> GetFlashcards(int stackId)
		{
			try
			{
                using (var connection = new SqlConnection(ConnectionString))
                {
					connection.Open();
					string sql = "select * from flashcards where stackId = @Id";
					var flashcards = connection.Query<Flashcard>(sql, new { Id = stackId });
					return flashcards;
                }

            }
			catch(Exception ex) {
                Console.WriteLine($"There was a problem retrieving stacks: {ex.Message}");
                return new List<Flashcard>();
            }
		}

		internal void InsertStudySession(StudySession session)
		{
			try
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					string sql = @"
					INSERT INTO StudySessions (Questions, CorrectAnswers, StackId, Time, Date)
					VALUES (@Questions, @CorrectAnswers, @StackId, @Time, @Date);";

					connection.Execute(sql, new { session.Questions, session.CorrectAnswers, session.StackId, session.Time, session.Date });
                }
			}
			catch(Exception ex)
			{
                Console.WriteLine($"There was a problem with the study session: {ex.Message}");
            }
        }

		internal List<StudySessionDTO> GetStudySessionData()
		{
			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				string sql = @"
				SELECT
					 s.Name as StackName,
					 ss.Date,
					 ss.Questions,
					 ss.CorrectAnswers,
					 ss.Percentage,
					 ss.Time
				 FROM
					 StudySessions ss
				 INNER JOIN
					 Stacks s ON ss.StackId = s.Id
				";
				return connection.Query<StudySessionDTO>(sql).ToList();
			}
		}
	}
}

