using System.ComponentModel;
using System.Configuration;

namespace RX_FFT.Properties {
    
    
    // Diese Klasse erm�glicht die Behandlung bestimmter Ereignisse der Einstellungsklasse:
    //  Das SettingChanging-Ereignis wird ausgel�st, bevor der Wert einer Einstellung ge�ndert wird.
    //  Das PropertyChanged-Ereignis wird ausgel�st, nachdem der Wert einer Einstellung ge�ndert wurde.
    //  Das SettingsLoaded-Ereignis wird ausgel�st, nachdem die Einstellungswerte geladen wurden.
    //  Das SettingsSaving-Ereignis wird ausgel�st, bevor die Einstellungswerte gespeichert werden.
    internal sealed partial class Settings {
        
        public Settings() {
            // // Heben Sie die  Auskommentierung der unten angezeigten Zeilen auf, um Ereignishandler zum Speichern und �ndern von Einstellungen hinzuzuf�gen:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e) {
            // F�gen Sie hier Code zum Behandeln des SettingChangingEvent-Ereignisses hinzu.
        }
        
        private void SettingsSavingEventHandler(object sender, CancelEventArgs e) {
            // F�gen Sie hier Code zum Behandeln des SettingsSaving-Ereignisses hinzu.
        }
    }
}
