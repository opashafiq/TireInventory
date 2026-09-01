namespace TireInventory.Helpers
{
    public static class Messages
    {
        public static string GetRefKeyErrorMessage(string objectName,string probableReferenceBy)
        {
            if (probableReferenceBy=="")
                return $"Cannot delete {objectName} because it is referenced by other records.";
            else
                return $"Cannot delete {objectName} because it is referenced by {probableReferenceBy}.";
        }
    }
}
