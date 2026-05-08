namespace ChibitsLink.main.cs.exception;

/// <summary>
/// Excepción lanzada cuando un documento no se encuentra en Firestore.
/// </summary>
public class RecordNotFoundException : DatabaseException
{
    public RecordNotFoundException(string collection, string id)
        : base($"El registro con ID '{id}' no fue encontrado en la colección '{collection}'.", collection, id) { }
}
