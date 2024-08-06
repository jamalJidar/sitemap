namespace app.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime DateCrete { get; set; }
        public string Slug { get; set; }
        public string _slug
        {
            get
            { return $"/{this.Title}/{this.Id}"; }
        }

        public List<Post> posts()
        {
            List<Post> list = new List<Post>();
            for (int i = 0; i <= 1023; i++)
            {
                list.Add(new Post()
                {
                    Id = i,
                    DateCrete = DateTime.Now.AddHours(i),
                    Title = $"title{i}"
                });
            }
            return list;
        }
    }

}