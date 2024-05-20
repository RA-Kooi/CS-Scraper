using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Data;

#nullable enable

[Serializable]
public class Item
{
	public string ID { get; set; } = "";

	[JsonPropertyName("product_type")]
	public string ProductType { get; set; } = "";

	[JsonPropertyName("jpn_card_no")]
	public string JpnCardNr { get; set; } = "";

	[JsonPropertyName("card_no")]
	public string CardNr { get; set; } = "";

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("color")]
	public string Color { get; set; } = "";

	[JsonPropertyName("card_type")]
	public string CardType { get; set; } = "";

	[JsonPropertyName("rarity")]
	public string Rarity { get; set; } = "";

	[JsonPropertyName("cost")]
	public string Cost { get; set; } = "";

	[JsonPropertyName("level")]
	public string Level { get; set; } = "";

	[JsonPropertyName("limits")]
	public string Limits { get; set; } = "";

	[JsonPropertyName("master")]
	public string Master { get; set; } = "";

	[JsonPropertyName("LRIG_SIGNI_type")]
	public string LrigSigniType { get; set; } = "";

	[JsonPropertyName("guard_coin_timing")]
	public string Timing { get; set; } = "";

	[JsonPropertyName("grow_cost")]
	public string GrowCost { get; set; } = "";

	[JsonPropertyName("power")]
	public string Power { get; set; } = "";

	[JsonPropertyName("content")]
	public string Content { get; set; } = "";

	[JsonPropertyName("power_text")]
	public string PowerText { get; set; } = "";

	[JsonPropertyName("fllabor_text")]
	public string FlavorText { get; set; } = "";

	[JsonPropertyName("artist")]
	public string Artist { get; set; } = "";

	[JsonPropertyName("flg")]
	public string Flag { get; set; } = "";

	[JsonPropertyName("subtype")]
	public string SubType { get; set; } = "";

	[JsonPropertyName("sdate")]
	public string Timestamp { get; set; } = "";
}

[Serializable]
public class ItemSearch
{
	[JsonPropertyName("items")]
	public List<Item> Items { get; set; } = new();

	[JsonPropertyName("count")]
	public string Total { get; set; } = "";
}
