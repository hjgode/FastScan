using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace FastScan
{
    /*
     * usage
     * in app.xaml.cs define global static var
                public static AppSoundEffects mySoundEffects;
     * just before Window.Current.Activate();
                // My Application Sound Effects
                mySoundEffects = new AppSoundEffects();
     * to play use:
        // Play a sound effect
        App.mySoundEffects.Play(SoundEfxEnum.Check);

        // Or asynchronously
        await App.mySoundEffects.Play(SoundEfxEnum.Check);
    */
    public class AppSoundEffects
    {
        public enum SoundEfxEnum
        {
            SUCCESS,
            ERROR,
            Bleep,
            ComputerError,
        }

        private Dictionary<SoundEfxEnum, MediaElement> effects;
        
        public AppSoundEffects()
        {
            effects = new Dictionary<SoundEfxEnum, MediaElement>();
            LoadEfx();
        }

        private async void LoadEfx()
        {
            effects.Add(SoundEfxEnum.SUCCESS, await LoadSoundFile("Success.wav"));
            effects.Add(SoundEfxEnum.ERROR, await LoadSoundFile("Error.wav"));
            effects.Add(SoundEfxEnum.Bleep, await LoadSoundFile("Bleep.wav"));
            effects.Add(SoundEfxEnum.ComputerError, await LoadSoundFile("ComputerError.wav"));
        }

        private async Task<MediaElement> LoadSoundFile(string v)
        {
            MediaElement snd = new MediaElement();

            snd.AutoPlay = false;
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync(v);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            snd.SetSource(stream, file.ContentType);
            snd.AutoPlay = false;
            snd.IsLooping = false;
            snd.IsMuted = false;            
            return snd;
        }

        //do not use, only every second sound does play
        public async Task PlayAsync(SoundEfxEnum efx)
        {
            var mediaElement = effects[efx];

            await mediaElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                mediaElement.Stop();
                mediaElement.Position = new TimeSpan(0);
                mediaElement.Play();
            });
        }

        public void Play(SoundEfxEnum efx)
        {
            var mediaElement = effects[efx];
            mediaElement.Play();
        }
        public void PlaySynced(SoundEfxEnum efx)
        {
            var mediaElement = effects[efx];

            mediaElement.Stop();
            mediaElement.Play();

        }
    }
}
