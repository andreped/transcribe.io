using System.Collections.Specialized;
using Microsoft.Maui.Adapters;
using Whisper.net.Ggml;

namespace transcribe.io.Models;

public class SectionedWhisperDownloadAdapter : VirtualListViewAdapterBase<WhisperDownloadSection, WhisperDownload>
{
    public readonly IList<WhisperDownloadSection> Items;
    
    public SectionedWhisperDownloadAdapter(IList<WhisperDownloadSection> items)
        : base()
    {
        this.Items = items;
    }
    
    public override int GetNumberOfSections()
        => Items.Count;

    public override int GetNumberOfItemsInSection(int sectionIndex)
        => Items[sectionIndex].Count;

    public override WhisperDownload GetItem(int sectionIndex, int itemIndex)
        => Items[sectionIndex][itemIndex];
    
    public override WhisperDownloadSection GetSection(int sectionIndex)
        => Items[sectionIndex];

    public void AddItems(List<WhisperDownload> downloads)
    {
        var groupedItems = downloads.GroupBy(n => n.Model.GgmlType);

        foreach (var groupedItemList in groupedItems)
        {
            var section = Items.FirstOrDefault(s => s.Type == groupedItemList.Key);

            if (section is null)
            {
                section = new WhisperDownloadSection() { Type = groupedItemList.Key };
                Items.Add(section);
            }

            section.AddRange(groupedItemList);
        }
    }
    
    public void AddItem(WhisperDownload itemName)
    {
        var section = Items.FirstOrDefault(s => s.Type == itemName.Model.GgmlType);

        if (section is null)
        {
            section = new WhisperDownloadSection() { Type = itemName.Model.GgmlType };
            Items.Add(section);
        }

        section.Add(itemName);
        InvalidateData();
    }

    public void RemoveItem(int sectionIndex, int itemIndex)
    {
        var section = Items.ElementAtOrDefault(sectionIndex);

        if (section is null)
            return;

        section.RemoveAt(itemIndex);

        if (section.Count <= 0)
            Items.RemoveAt(sectionIndex);

        InvalidateData();
    }
}

public class WhisperDownloadSection : List<WhisperDownload>
{
    public GgmlType Type { get; set; }
}