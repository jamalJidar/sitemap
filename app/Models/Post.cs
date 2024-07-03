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
            { return $"/post/{this.Id}/{this.Title}"; }
        }

        public List<Post> posts()
        {
            List<Post> list = new List<Post>();
            for (int i = 1; i < 353; i++)
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