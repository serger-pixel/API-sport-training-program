using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_sprot_training_program.Models
{
    public class TrainingProgram
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public long Id { get; set; }

        [BsonDefaultValue("Программа")]
        public String Title { get; set; }

        [BsonDefaultValue("Отсутсвует")]
        public String Description { get; set; }

        [BsonDefaultValue(0)]
        public int DurationInWeek { get; set; }

        [BsonDefaultValue(0)]
        public int CntInWeek { get; set; }

    }
}
