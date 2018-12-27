using NFluent;
using Ninject;
using NUnit.Framework;
using SpExecuteSqlTransformer.Core;
using SpExecuteSqlTransformer.Core.Manipulators;
using SpExecuteSqlTransformer.Core.Parser;
using System;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class TransformationManagerTests
    {
        [Test]
        public void IntegrationTest_LongStatement()
        {
            var statement = @"exec sp_executesql N'SELECT DISTINCT TOP 200 [FirstSampleTable].*  FROM [FirstSampleTable]
  where
    ([FirstSampleTable].[IsActive] = @param_0_) And
    ([FirstSampleTable].[ID] IN (
SELECT [SecondSampleTable].[ID] FROM [SecondSampleTable]
  where
    CONTAINS ([SecondSampleTable].[Subject], @param_1_)
) Or
    [FirstSampleTable].[ID] IN (
SELECT [ThirdSampleTable].[ID] FROM [ThirdSampleTable]
  where
    CONTAINS ([ThirdSampleTable].[Subject], @param_2_)
) Or
    [FirstSampleTable].[ID] IN (
SELECT [FourthSampleTable].[ID] FROM [FourthSampleTable]
  where
    CONTAINS ([FourthSampleTable].[Subject], @param_3_)
)) And
    (FirstSampleTable.[Created] <= @param_4__ToValue) And
    (
 EXISTS (
SELECT [dbo].[FifthSampleTable].[ID] FROM [dbo].[FifthSampleTable]
  where
    [FifthSampleTable].[ID] = [FirstSampleTable].ID AND (AppReference IS NULL OR AppReference IN (''af342a57-f60c-4e93-8646-d18397a8b7e6''))
)) And
    [FirstSampleTable].[UserID] IN (''50e9cef6-35e1-40e7-8b2c-1ce2f0ae5d33'',''00000000-0000-0000-0000-000000000001'') And
    [FirstSampleTable].ID not IN (select x.ID from [CommonFourthSampleTableView] x where [CommonFourthSampleTableState] in (@CommonFourthSampleTableState_Completed, @CommonFourthSampleTableState_Progress)) And
    [FirstSampleTable].ID not IN (select x.ID from [BaseSecondSampleTableView] x where [SecondSampleTableState] in (@SecondSampleTableState_Completed, @SecondSampleTableState_Progress))
  order by [FirstSampleTable].[Created] desc
 option(recompile);',N'@param_0_ bit,@param_1_ nvarchar(4000),@param_2_ nvarchar(4000),@param_3_ nvarchar(4000),@param_4__ToValue datetime,@CommonFourthSampleTableState_Completed varchar(20),@CommonFourthSampleTableState_Progress varchar(20),@SecondSampleTableState_Completed int,@SecondSampleTableState_Progress int',@param_0_=0,@param_1_=N'""asdf""',@param_2_=N'""asdf""',@param_3_=N'""asdf""',@param_4__ToValue='2018-01-06 18:13:20.493',@CommonFourthSampleTableState_Completed='Completed',@CommonFourthSampleTableState_Progress='Progress',@SecondSampleTableState_Completed=4,@SecondSampleTableState_Progress=5";

            var result = ManipulateSqlString(statement);

            Check.That(result.Exception).IsNull();
            Check.That(result.ErrorMessage).IsNull();
            Check.That(result.ManipulatorResults).CountIs(3);
            Check.That(result.ManipulatorResults).ContainsOnlyElementsThatMatch(manipulatorResult => !manipulatorResult.HasError);
            Check.That(result.TokenList).IsNotNull();
            Check.That(result.ParseResult).IsNotNull();
            Check.That(result.ResultString).IsEqualTo(@"SELECT DISTINCT TOP 200 [FirstSampleTable].*
FROM [FirstSampleTable]
WHERE ([FirstSampleTable].[IsActive] = 0)
	AND (
		[FirstSampleTable].[ID] IN (
			SELECT [SecondSampleTable].[ID]
			FROM [SecondSampleTable]
			WHERE CONTAINS (
					[SecondSampleTable].[Subject]
					,N'""asdf""'
					)
			)
		OR [FirstSampleTable].[ID] IN (
			SELECT [ThirdSampleTable].[ID]
			FROM [ThirdSampleTable]
			WHERE CONTAINS (
					[ThirdSampleTable].[Subject]
					,N'""asdf""'
					)
			)
		OR [FirstSampleTable].[ID] IN (
			SELECT [FourthSampleTable].[ID]
			FROM [FourthSampleTable]
			WHERE CONTAINS (
					[FourthSampleTable].[Subject]
					,N'""asdf""'
					)
			)
		)
	AND (FirstSampleTable.[Created] <= '2018-01-06 18:13:20.493')
	AND (
		EXISTS (
			SELECT [dbo].[FifthSampleTable].[ID]
			FROM [dbo].[FifthSampleTable]
			WHERE [FifthSampleTable].[ID] = [FirstSampleTable].ID
				AND (
					AppReference IS NULL
					OR AppReference IN ('af342a57-f60c-4e93-8646-d18397a8b7e6')
					)
			)
		)
	AND [FirstSampleTable].[UserID] IN (
		'50e9cef6-35e1-40e7-8b2c-1ce2f0ae5d33'
		,'00000000-0000-0000-0000-000000000001'
		)
	AND [FirstSampleTable].ID NOT IN (
		SELECT x.ID
		FROM [CommonFourthSampleTableView] x
		WHERE [CommonFourthSampleTableState] IN (
				'Completed'
				,'Progress'
				)
		)
	AND [FirstSampleTable].ID NOT IN (
		SELECT x.ID
		FROM [BaseSecondSampleTableView] x
		WHERE [SecondSampleTableState] IN (
				4
				,5
				)
		)
ORDER BY [FirstSampleTable].[Created] DESC
OPTION (RECOMPILE);
");
        }

        [Test]
        public void IntegrationTest_ShortStatement()
        {
            var statement = @"exec sp_executesql N'UPDATE [User] SET [Name] = @Name, DateOfBirth = @DateOfBirth 
                WHERE [ID] = @ID and [ParentID] = ''1234'';'
                , N'@ID uniqueidentifier,@Name nvarchar(100),@DateOfBirth datetime'
                ,@ID = '00000002-0000-0000-0000-000000000002',@Name = 'Alex',
                @DateOfBirth = '2017-12-13 12:02:43.227'";

            var result = ManipulateSqlString(statement);

            Check.That(result.Exception).IsNull();
            Check.That(result.ErrorMessage).IsNull();
            Check.That(result.TokenList).IsNotNull();
            Check.That(result.ParseResult).IsNotNull();

            Check.That(result.ManipulatorResults).CountIs(3);
            Check.That(result.ManipulatorResults).ContainsOnlyElementsThatMatch(manipulatorResult => !manipulatorResult.HasError);
            Check.That(result.ManipulatorResults[0].ManipulatorType).IsEqualTo(typeof(StringUnwrapper));
            Check.That(result.ManipulatorResults[0].InputString).IsEqualTo(@"N'UPDATE [User] SET [Name] = @Name, DateOfBirth = @DateOfBirth 
                WHERE [ID] = @ID and [ParentID] = ''1234'';'");
            Check.That(result.ManipulatorResults[0].ResultString).IsEqualTo(@"UPDATE [User] SET [Name] = @Name, DateOfBirth = @DateOfBirth 
                WHERE [ID] = @ID and [ParentID] = '1234';");
            Check.That(result.ManipulatorResults[0].Exception).IsNull();

            Check.That(result.ManipulatorResults[1].ManipulatorType).IsEqualTo(typeof(ParamReplacer));
            Check.That(result.ManipulatorResults[1].InputString).IsEqualTo(@"UPDATE [User] SET [Name] = @Name, DateOfBirth = @DateOfBirth 
                WHERE [ID] = @ID and [ParentID] = '1234';");
            Check.That(result.ManipulatorResults[1].ResultString).IsEqualTo(@"UPDATE [User] SET [Name] = 'Alex', DateOfBirth = '2017-12-13 12:02:43.227' 
                WHERE [ID] = '00000002-0000-0000-0000-000000000002' and [ParentID] = '1234';");
            Check.That(result.ManipulatorResults[1].Exception).IsNull();

            Check.That(result.ManipulatorResults[2].ManipulatorType).IsEqualTo(typeof(Formatter));
            Check.That(result.ManipulatorResults[2].InputString).IsEqualTo(@"UPDATE [User] SET [Name] = 'Alex', DateOfBirth = '2017-12-13 12:02:43.227' 
                WHERE [ID] = '00000002-0000-0000-0000-000000000002' and [ParentID] = '1234';");
            Check.That(result.ManipulatorResults[2].ResultString).IsEqualTo(@"UPDATE [User]
SET [Name] = 'Alex'
	,DateOfBirth = '2017-12-13 12:02:43.227'
WHERE [ID] = '00000002-0000-0000-0000-000000000002'
	AND [ParentID] = '1234';
");
            Check.That(result.ManipulatorResults[2].Exception).IsNull();

            Check.That(result.ResultString).IsEqualTo(
                @"UPDATE [User]
SET [Name] = 'Alex'
	,DateOfBirth = '2017-12-13 12:02:43.227'
WHERE [ID] = '00000002-0000-0000-0000-000000000002'
	AND [ParentID] = '1234';
");            
        }

        [Test]
        public void IntegrationTest_ParameterNameContainingOtherParametersName()
        {
            var statement = @"exec sp_executesql N'UPDATE [TestTable] SET [TestID] = @TestID, [TestIDClassID] = @TestIDClassID',
N'@TestID uniqueidentifier,@TestIDClassID varchar(100)'
,@TestID=NULL,@TestIDClassID=NULL";

            var result = ManipulateSqlString(statement);

            Check.That(result.ResultString).IsEqualTo(@"UPDATE [TestTable]
SET [TestID] = NULL
	,[TestIDClassID] = NULL
");
        }

        [Test]
        public void IntegrationTest_FormattingOnly()
        {
            var statement = "select * from users where name = 'foo' and index = 1";
            var result = ManipulateSqlString(statement);

            Check.That(result.Exception).IsInstanceOf<UnexpectedTokenException>();
            Check.That(result.Exception.Message).IsEqualTo("Unexpected token at 0. Expected: StringLiteral. Actual: Word.");
            Check.That(result.ErrorMessage).IsEqualTo("Parsing failed.");
            Check.That(result.TokenList).IsNotNull();
            Check.That(result.ParseResult).IsNotNull();

            Check.That(result.ResultString).IsEqualTo(@"SELECT *
FROM users
WHERE NAME = 'foo'
	AND INDEX = 1
");
        }
        
        [Test]
        public void IntegrationTest_WithError_ManipulatorFailed()
        {
            var statement = @"exec sp_executesql N'UPDATE [User] SET ";

            var result = ManipulateSqlString(statement);

            Check.That(result.Exception).IsNull();
            Check.That(result.ErrorMessage).IsNull();

            Check.That(result.ManipulatorResults[0].ManipulatorType).IsEqualTo(typeof(StringUnwrapper));
            Check.That(result.ManipulatorResults[0].HasError).IsTrue();
            Check.That(result.ManipulatorResults[0].Exception).IsInstanceOf<InvalidOperationException>();
            Check.That(result.ManipulatorResults[0].Exception.Message).IsEqualTo("String \"N'UPDATE [User] SET \" seems to not be a valid sql string");
            Check.That(result.TokenList).IsNotNull();
            Check.That(result.ParseResult).IsNotNull();
        }

        [Test]
        public void IntegrationTest_NotASqlString()
        {
            var statement = "foo bar asdf";
            var result = ManipulateSqlString(statement);
            Check.That(result.ResultString).IsEqualTo("foo bar asdf\r\n");
        }

        private TransformationResult ManipulateSqlString(string sqlString)
        {
            var manager = GetManager();
            return manager.TransformSqlString(sqlString);
        }

        private ITransformationManager GetManager()
        {
            return new StandardKernel(new IoCModule()).Get<ITransformationManager>();
        }
    }
}
