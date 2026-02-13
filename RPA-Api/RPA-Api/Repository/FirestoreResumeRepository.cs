using Google.Cloud.Firestore;
using RPA_Api.Model;

namespace RPA_Api.Repository
{
    public class FirestoreResumeRepository : IResumeRepository
    {
        private readonly FirestoreDb _db;

        public FirestoreResumeRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task CreateAsync(ResumeRecord record, CancellationToken ct)
        {
            // Collection: resumes
            var docRef = _db.Collection("resumes").Document(record.ResumeId);
            await docRef.SetAsync(record, cancellationToken: ct);
        }
    }

}
