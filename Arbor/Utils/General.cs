namespace Arbor.Utils;

public static class General
{
    public static bool AttemptWithRetryOnException<TException>(this Action action, int attempts = 10, bool throwOnFailure = true)
        where TException : Exception
    {
        while (true)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                if (e is not TException)
                    throw;

                if (attempts-- == 0)
                {
                    if (throwOnFailure)
                        throw;

                    return false;
                }

                Console.WriteLine($"Operation failed ({e.Message}). Retrying {attempts} more times...");
            }
            
            Thread.Sleep(250);
        }
    }
}
