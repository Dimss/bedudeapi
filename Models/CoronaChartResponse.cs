namespace BeDudeApi.Models
{
    public class CoronaSatausResponse
    {
        public CoronaSatausResponse(string date, decimal quantity, string quantityType)
        {
            this.date = date;
            this.quantity = quantity;
            this.quantityType = quantityType;

        }


        public string date { get; set; }

        public decimal quantity { get; set; }

        public string quantityType { get; set; }
    }
}