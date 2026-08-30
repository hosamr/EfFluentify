namespace EFFluentify.Tests.Helpers
{
    public static class TestingHelper
    {
        public static string FindSolutionRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !dir.GetFiles("*.sln").Any())
                dir = dir.Parent;
            if (dir == null) throw new InvalidOperationException("Could not find solution root (no .sln up the tree).");
            return dir.FullName;
        }

        public static string Normalize(string s)
        {
            return string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
        }
    }
}
