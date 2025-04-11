using System.Collections.ObjectModel;
using NotesApp.Models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace NotesApp
{
    public partial class MainPage : ContentPage
    {
        private const string FileName = "notes.json";
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();
        public ICommand DeleteNoteCommand { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            LoadNotes();
            NotesCollection.ItemsSource = Notes;
            Notes.CollectionChanged += Notes_CollectionChanged;

            DeleteNoteCommand = new Command<Note>(async (note) => await ConfirmDeleteNote(note));
        }

        private async Task ConfirmDeleteNote(Note note)
        {
            bool isUserSure = await DisplayAlert("Delete Note?", "Please confirm you want to delete this note", "Delete", "Cancel");
            if (isUserSure && note != null)
            {
                Vibration.Vibrate(300);
                Notes.Remove(note);
                await SaveNotesAsync();
            }
        }

        private async void Notes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await SaveNotesAsync();
        }

        public async Task SaveNotesAsync()
        {
            var folder = FileSystem.AppDataDirectory;
            var file = Path.Combine(folder, FileName);
            var json = JsonSerializer.Serialize(Notes);
            await File.WriteAllTextAsync(file, json);
        }

        private void LoadNotes()
        {
            var folder = FileSystem.AppDataDirectory;
            var file = Path.Combine(folder, FileName);
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                var notes = JsonSerializer.Deserialize<ObservableCollection<Note>>(json);
                if (notes != null)
                {
                    foreach (var note in notes)
                    {
                        Notes.Add(note);
                    }
                }
            }
        }

        private async void OnAddNoteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditNotePage(Notes));
        }

        private async void OnEditNoteTapped(object sender, EventArgs e)
        {
            var stackLayout = sender as StackLayout;
            if (stackLayout?.BindingContext is Note noteToEdit)
            {
                await Navigation.PushAsync(new AddEditNotePage(Notes, noteToEdit));
            }
        }
    }
}
