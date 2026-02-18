namespace ChibitsLink.main.cs.model;

using System.Collections.Generic;
/*
 * entidad de party estas son las lobbys
 */
public class Party
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public List<int> PlayerIds { get; set; } = new List<int>();
}