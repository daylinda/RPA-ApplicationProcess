
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Mvc;
using RPA_Api.Model;

namespace RPA_Api.Repository
{
    public class FirestoreUserRepository:IUserRepository
    {
        FirestoreDb _db;
        public FirestoreUserRepository(FirestoreDb db) { 
            _db = db;
        }

        public async Task CreateAsync(UserRecord record)
        {
            var docRef = _db.Collection("users").Document(record.Id);
            await docRef.SetAsync(record, cancellationToken: CancellationToken.None);
            
        }
    }
}
