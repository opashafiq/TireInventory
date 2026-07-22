namespace TireInventory.Helpers
{
    public class CommonFunctions
    {
        public static long? GenerateTransactionID()
        {
            // 1. Get current time as HHmmss
            string timePart = DateTime.Now.ToString("HHmmss");

            // 2. Generate random number up to 1,000,000
            int randomPart = Random.Shared.Next(0, 1000000);

            // 3. Combine them
            return (long?)Convert.ToInt64($"{timePart}{randomPart}");
        }
    }
}
