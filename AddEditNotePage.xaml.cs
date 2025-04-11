using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotesApp.Models;

namespace NotesApp
{
    public partial class AddEditNotePage : ContentPage
    {
        private ObservableCollection<Note> _notes;
        private Note _currentNote;
        private bool _isNewNote = true;
        public bool IsSaving = false;

        
        public AddEditNotePage(ObservableCollection<Note> notes) //constructor for new note
        {
            InitializeComponent();
            _notes = notes;
            _currentNote = new Note();
            BindEntries();
        }

        public AddEditNotePage(ObservableCollection<Note> notes, Note note) //constructor for editing an existing note
        {
            InitializeComponent();
            _notes = notes;
            _currentNote = note;
            _isNewNote = false;
            BindEntries();
            LoadNoteDetails(note);
        }

        private void BindEntries()
        {
            TitleEntry.Text = _currentNote.Title;
            ContentEditor.Text = _currentNote.Content;
        }

        private async void OnSaveNoteClicked(object sender, EventArgs e)
        {
            if (IsSaving)
                return;

            IsSaving = true;
            SaveIndicator.IsRunning = true;
            SaveIndicator.IsVisible = true;

            _currentNote.Title = TitleEntry.Text;
            _currentNote.Content = ContentEditor.Text;
            _currentNote.Location = await GetLocationAsync();

            try
            {
                if (_isNewNote)
                {
                    _notes.Insert(0, _currentNote);
                }
                else
                {
                    int index = _notes.IndexOf(_currentNote);
                    if (index != -1)
                    {
                        _notes.RemoveAt(index);
                    }

                    var newNote = new Note
                    {
                        Title = _currentNote.Title,
                        Content = _currentNote.Content,
                        Location = _currentNote.Location,
                        ImagePath = _currentNote.ImagePath
                    };

                    _notes.Insert(0, newNote);
                }

                await Navigation.PopAsync();
            }
            finally
            {
                IsSaving = false;
                SaveIndicator.IsRunning = false;
                SaveIndicator.IsVisible = false;
                Vibration.Vibrate(300);
            }
        }


        private async Task<string> GetLocationAsync()
        {
            try
            {
                await CheckAndRequestLocationPermission();
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        string city = placemark.Locality;
                        string adminArea = placemark.AdminArea;
                        string countryName = placemark.CountryName;

                        return $"{city}, {adminArea}, {countryName}";
                    }
                    return "Specific location not found";
                }
                else
                {
                    return "Location unknown";
                }
            }
            catch (FeatureNotSupportedException)
            {
                return "Location not supported on device";
            }
            catch (PermissionException)
            {
                return "Location permission not granted";
            }
            catch (Exception)
            {
                return "Error getting location";
            }
        }

        private async Task CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            if (status != PermissionStatus.Granted)
            {
                throw new PermissionException("Location permission not granted");
            }
        }

        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                await CheckAndRequestCameraPermission();

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    var filePath = await SavePhotoToFile(photo);
                    _currentNote.ImagePath = filePath;
                    LoadNoteDetails(_currentNote);
                }
            }
            catch (PermissionException)
            {
                await DisplayAlert("Permission Denied", "Camera permission is not granted.", "OK");
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Error", "Camera not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task CheckAndRequestCameraPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }
            if (status != PermissionStatus.Granted)
            {
                throw new PermissionException("Camera permission not granted");
            }
        }

        private async Task<string> SavePhotoToFile(FileResult photo)
        {
            var newFileName = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}.jpg");
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFileName))
            {
                await stream.CopyToAsync(newStream);
            }
            return newFileName;
        }

        private void LoadNoteDetails(Note note)
        {
            if (!string.IsNullOrEmpty(note.ImagePath) && File.Exists(note.ImagePath))
            {
                PhotoImage.Source = ImageSource.FromFile(note.ImagePath);
            }
            else
            {
                PhotoImage.Source = null;
            }
        }
    }
}