namespace StudentManager
{
    public class Student
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public double Score { get; set; }

        public string Status => Score >= 5.0 ? "Đạt" : "Không đạt";
    }
}