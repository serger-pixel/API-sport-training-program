using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_sprot_training_program.Models
{
    public class TrainingProgram
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public int DurationInWeek { get; set; }

        public int CntInWeek { get; set; }

    }
}
