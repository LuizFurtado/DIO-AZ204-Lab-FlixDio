namespace FnPostDatabase
{
    internal class MovieRequest
    {
        public string Id { get { return Guid.NewGuid().ToString(); } }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Video { get; set; }
        public string Thumb { get; set; }
    }
}
