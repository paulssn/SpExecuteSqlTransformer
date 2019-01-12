namespace SpExecuteSqlTransformer.Model
{
    public interface IClipboard
    {
        bool ContainsText();
        string GetText();
        void SetText(string text);
    }
}
