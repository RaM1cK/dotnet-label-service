using System.ComponentModel.DataAnnotations;

namespace dotnet_label_service.Models
{
    public class Track
    {
        public Guid Id { get; private set; }
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string Label { get; set; }
        public DateTime CreatedAt { get; private set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}

//{
//  "id": "be9512d5-82d7-4b9c-b0bd-48721d23119b",
//  "title": "Cadillac",
//  "artist": "MORGENSHTERN",
//  "label": "label1",
//  "createdAt": "2026-09-20T10:59:11.780411Z"
//}
