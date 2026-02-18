using Plugin.CloudFirestore;

namespace ChibitsLink.main.repository;

/// <summary>
/// Provee el acceso a la instancia de Cloud Firestore.
/// </summary>
public class FirebaseConnection
{
    public IFirestore Firestore => CrossCloudFirestore.Current.Instance;
    public Plugin.FirebaseAuth.IAuth Auth => Plugin.FirebaseAuth.CrossFirebaseAuth.Current.Instance;

    public FirebaseConnection()
    {
        // En MAUI con Plugin.CloudFirestore, la inicialización suele ser automática 
        // a través de los archivos de configuración nativos (google-services.json / GoogleService-Info.plist)
    }
}