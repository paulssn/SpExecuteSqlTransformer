namespace SpExecuteSqlTransformer.Core
{
    public interface ITransformationManager
    {
        TransformationResult TransformSqlString(string sqlString);
    }
}