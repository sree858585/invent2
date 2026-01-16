namespace HIVTraining_Vue.Server.Models
{
    public class SubjectTopic
    {
        public int SubjectSysId { get; set; }
        public int TopicCode { get; set; }

        public Subject? Subject { get; set; }
        public LkTopic? Topic { get; set; }
    }
}