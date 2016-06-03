
' Copyright(C) 2016 vistaero
'
' This program Is free software: you can redistribute it And/Or modify
' it under the terms Of the GNU General Public License As published by
' the Free Software Foundation, either version 3 Of the License, Or
' (at your option) any later version.

' This program Is distributed In the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty Of
' MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with this program. If Not, see <http://www.gnu.org/licenses/>.


Imports System.ComponentModel
Imports System.Deployment.Application
Imports System.IO
Imports System.Net

Class MainWindow

    Private MessageBox As New Xceed.Wpf.Toolkit.MessageBox

    Private LatestVersionChecker As New BackgroundWorker
    Private LatestVersion As String

    Private PathSeparator As String = IO.Path.DirectorySeparatorChar

    Private LanguageFolder As String
    Private LanguageFilesList As New List(Of String)

    Private EnglishPath As String
    Private CurrentLanguage As String

    Private FileContent As Object

    Private SavePath As String

    Private OriginalStrings As New List(Of String)
    Private TranslatedStrings As New List(Of String)

    Private CurrentString As Integer = 0

    Private WarningText As String = "## do not remove or change '%s' as these are needed to insert names, commands and config options"
    Private TextToSave As String

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        LatestVersionChecker.WorkerSupportsCancellation = True
        AddHandler LatestVersionChecker.DoWork, AddressOf BackgroundWorker1_DoWork
        AddHandler LatestVersionChecker.RunWorkerCompleted, AddressOf LatestVersionChecker1_RunWorkerCompleted

        LatestVersionChecker.RunWorkerAsync()

        Try
            If IO.Directory.Exists(My.Settings.languageFolder) Then
                LanguageFolder = My.Settings.languageFolder
                ListFiles()
                LoadEnglish()
            Else
                SelectFolder()

            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Function GetVersionNumber()
        Dim currentVersion As String

        Try
            currentVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString
        Catch ex As Exception
            currentVersion = "Debug Version"
        End Try

        Return currentVersion

    End Function

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim address As String = "https://www.dropbox.com/s/1zo7qar8ehst9z6/version.txt?dl=1"
        Dim client As WebClient = New WebClient()
        Dim reader As StreamReader = New StreamReader(client.OpenRead(address))
        LatestVersion = reader.ReadToEnd

    End Sub

    Private ReportNoUpdate As Boolean = False

    Private Sub LatestVersionChecker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        Dim ThisIsReleaseVersion As Boolean = True
        If GetVersionNumber() = "Debug Version" Then ThisIsReleaseVersion = False
        Dim ThereIsInternet As Boolean = My.Computer.Network.Ping("108.160.172.238", 10000)
        Dim LatestVersionEqualsCurrentVersion As Boolean
        If LatestVersion.Equals(GetVersionNumber()) Then LatestVersionEqualsCurrentVersion = True

        If ThisIsReleaseVersion = False AndAlso ReportNoUpdate = True Then
            MessageBox.Show("Can't check for updates when using a Debug Version", "tlTranslator Updater", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        If ThisIsReleaseVersion = True AndAlso ThereIsInternet = True AndAlso LatestVersionEqualsCurrentVersion = True AndAlso ReportNoUpdate = True Then
            MessageBox.Show("No updates found", "tlTranslator Updater", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        If ThisIsReleaseVersion = True AndAlso ThereIsInternet = True AndAlso LatestVersionEqualsCurrentVersion = False Then
            Dim window As New NewVersionInfo
            window.ShowDialog()
        End If

    End Sub

    Private Sub LoadButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadButton.Click
        SelectFolder()

    End Sub

    Private Sub ClearVars()
        LanguageFolder = ""
        LanguageFilesList.Clear()
        LanguageFilesListBox.Items.Refresh()
        EnglishPath = ""
        CurrentLanguage = ""
        FileContent = ""
        SavePath = ""
        OriginalStrings.Clear()
        TranslatedStrings.Clear()
        CurrentString = 0
        TextToSave = ""
        CurrentStringLabel.Text = 0
        OriginalString.Text = ""
        TranslatedString.Text = ""
    End Sub

    Private Sub SelectFolder()
        Dim OpenFolderDialog As New Avalon.Windows.Dialogs.FolderBrowserDialog
        OpenFolderDialog.ShowStatusText = True
        OpenFolderDialog.Title = "Find the ""\plugins\TARDIS\language"" folder of your server."

        OpenFolderDialog.ShowDialog()
        If System.IO.Directory.Exists(OpenFolderDialog.SelectedPath) Then
            If System.IO.File.Exists(OpenFolderDialog.SelectedPath & PathSeparator & "en.yml") Then
                ClearVars()

                LanguageFolder = OpenFolderDialog.SelectedPath
                My.Settings.languageFolder = LanguageFolder
                My.Settings.Save()

                ListFiles()
                LoadEnglish()

            Else
                MessageBox.Show("That's not a valid TARDIS Plugin languages folder. ""en.yml"" is missing.", "Not valid", MessageBoxButton.OK, MessageBoxImage.Error)

            End If

        End If

    End Sub

    Private Sub ListFiles()
        Dim NoListableFiles() As String = {"en", "chameleon_guis", "signs"}

        For Each item In System.IO.Directory.EnumerateFiles(LanguageFolder)

            Dim itemName = IO.Path.GetFileNameWithoutExtension(item)
            If Not LanguageFilesList.Contains(itemName) Then
                If Not NoListableFiles.Contains(itemName) Then
                    LanguageFilesList.Add(itemName)
                End If
            End If

        Next
        LanguageFilesListBox.ItemsSource = LanguageFilesList

        LanguageFilesListBox.Items.Refresh()

        FilePathLabel.Text = LanguageFolder

        AddLanguageCombobox.IsEnabled = True
        OpenLangFolder.IsEnabled = True

    End Sub

    Private Sub LoadEnglish()

        Dim EnglishContent As Object

        Try
            EnglishContent = System.IO.File.ReadAllLines(LanguageFolder & PathSeparator & "en.yml")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            My.Settings.Reset()
            My.Settings.Save()

        End Try

        For Each line In EnglishContent
            If line <> "" And Not line.ToString.StartsWith("#") Then
                OriginalStrings.Add(line)

            End If
        Next

    End Sub

    Private Sub LanguageFilesListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles LanguageFilesListBox.SelectionChanged

        CurrentLanguage = LanguageFolder & PathSeparator & LanguageFilesListBox.SelectedItem & ".yml"
        CurrentStringLabel.Text = 0

        If IO.File.Exists(CurrentLanguage) Then
            FileContent = ""
            SavePath = CurrentLanguage
            TranslatedStrings = New List(Of String)
            CurrentString = 0
            TextToSave = vbNewLine & WarningText

            FilePathLabel.Text = CurrentLanguage


            Try
                FileContent = System.IO.File.ReadAllLines(CurrentLanguage)
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)

                Exit Sub

            End Try

            For Each Line In FileContent
                If Line <> "" And Not Line.ToString.StartsWith("#") Then
                    TranslatedStrings.Add(Line)
                End If
            Next

            GoToString(0)

            NextButton.IsEnabled = True
            PreviousButton.IsEnabled = True
            SaveButton.IsEnabled = True
            CurrentStringLabel.IsEnabled = True
            SearchBox.IsEnabled = True


            TotalStringsLabel.Content = OriginalStrings.Count - 1

            TranslatedString.Focus()
            PlaceTheCaret()

            CompareFiles()

        End If

    End Sub

    Private Sub CompareFiles()
        If OriginalStrings.Count <> TranslatedStrings.Count Then
            Dim MissingStrings As Integer = OriginalStrings.Count - TranslatedStrings.Count
            Dim addToMessageBox As String

            If MissingStrings > 0 Then
                addToMessageBox = "It is missing " & MissingStrings & " strings."
            ElseIf MissingStrings < 0 Then
                addToMessageBox = "It has " & MissingStrings * -1 & " additional(s) string(s)."
            End If
            MessageBox.Show("This translation does not have the right number of strings. " & addToMessageBox & vbNewLine & vbNewLine & "Start the server, do ""/tardisadmin language " & LanguageFilesListBox.SelectedItem & """ and restart the server. TARDIS plugin will update the translation file with the latest strings.", "", MessageBoxButton.OK, MessageBoxImage.Error)
        End If
    End Sub

    Private Sub NextButton_Click(sender As Object, e As RoutedEventArgs) Handles NextButton.Click
        GoNext()
    End Sub

    Private Sub PreviousButton_Click(sender As Object, e As RoutedEventArgs) Handles PreviousButton.Click
        GoPrevious()
    End Sub

    Private Sub TranslatedString_KeyDown(sender As Object, e As KeyEventArgs) Handles TranslatedString.PreviewKeyDown
        If CurrentLanguage <> "" Then

            If e.Key = Key.Up AndAlso Keyboard.IsKeyDown(Key.LeftCtrl) OrElse Keyboard.IsKeyDown(Key.RightCtrl) Then
                e.Handled = True
                NextResult()
                PlaceTheCaret()

            ElseIf e.Key = Key.Down AndAlso Keyboard.IsKeyDown(Key.LeftCtrl) OrElse Keyboard.IsKeyDown(Key.RightCtrl) Then
                e.Handled = True
                PreviousResult()
                PlaceTheCaret()

            ElseIf e.Key = Key.Enter Then
                TranslatedString.Focus()
                PlaceTheCaret()

            ElseIf e.Key = Key.S AndAlso (Keyboard.Modifiers And ModifierKeys.Control) = ModifierKeys.Control Then
                Save()

            End If

            If e.Key = Key.Up Then
                GoNext()

            ElseIf e.Key = Key.Down Then
                GoPrevious()
            End If

        End If

    End Sub

    Private Sub TranslatedString_KeyUp(sender As Object, e As KeyEventArgs) Handles TranslatedString.KeyUp
        If e.Key = Key.Down Then PlaceTheCaret()
    End Sub

    Private Sub CurrentStringLabel_TextChanged(sender As Object, e As TextChangedEventArgs) Handles CurrentStringLabel.TextChanged
        Try
            GoToString(CurrentStringLabel.Text)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub GoToString(ByVal StringNumber As Integer)
        If StringNumber >= 0 And StringNumber <= OriginalStrings.Count Then
            CurrentString = StringNumber
            OriginalString.Text = OriginalStrings(CurrentString)
            TranslatedString.Text = TranslatedStrings(CurrentString)
            CurrentStringLabel.Text = StringNumber
        End If

    End Sub

    Private Sub GoNext()
        If CurrentString <> OriginalStrings.Count - 1 Then
            TranslatedStrings(CurrentString) = TranslatedString.Text
            CurrentString += 1
            GoToString(CurrentString)

            PlaceTheCaret()
        End If
    End Sub

    Private Sub GoPrevious()
        If CurrentString <> 0 Then
            TranslatedStrings(CurrentString) = TranslatedString.Text
            CurrentString -= 1
            GoToString(CurrentString)

            PlaceTheCaret()
        End If
    End Sub

    Private Sub PlaceTheCaret()

        TranslatedString.Focus()

        Dim split As String() = TranslatedString.Text.Split(New [Char]() {":"c})
        For Each s As String In split
            If TranslatedString.Text.EndsWith("""") Then
                TranslatedString.CaretIndex = s.Length + 3
            Else
                TranslatedString.CaretIndex = s.Length + 2
            End If

            Exit For

        Next

    End Sub

    Private Sub GenerateText()
        TextToSave = ""
        TextToSave = WarningText

        TranslatedStrings(CurrentString) = TranslatedString.Text

        For Each item In TranslatedStrings
            TextToSave += vbNewLine & item
        Next

        TextToSave += vbNewLine
    End Sub

    Private Sub Save()
        GenerateText()
        Try
            System.IO.File.WriteAllText(CurrentLanguage, TextToSave, Text.Encoding.UTF8)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Save()
    End Sub

    Private Sub AboutButton_Click(sender As Object, e As RoutedEventArgs) Handles AboutButton.Click
        Dim AboutBox As New About("Version: " & GetVersionNumber())
        AboutBox.Show()

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If CurrentLanguage <> "" Then
            Dim result As MessageBoxResult = MessageBox.Show("Save before exit?", "Save File", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)
            Select Case result
                Case MessageBoxResult.Yes
                    Save()
                Case MessageBoxResult.No
                    Environment.Exit(0)
                Case Else
                    e.Cancel = True
                    Exit Select

            End Select

        End If
    End Sub

    Private Sub AddLanguageCombobox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles AddLanguageCombobox.SelectionChanged
        If AddLanguageCombobox.SelectedIndex <> 0 Then
            Dim selectedlang As String = DirectCast(AddLanguageCombobox.SelectedItem, ComboBoxItem).Content.ToString()
            Dim destino As String = LanguageFolder & PathSeparator & selectedlang & ".yml"

            Try
                System.IO.File.Copy(LanguageFolder & PathSeparator & "en.yml", destino)
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try

            ListFiles()
        End If

    End Sub

    Private Sub OpenLangFolder_Click(sender As Object, e As RoutedEventArgs) Handles OpenLangFolder.Click
        If System.IO.Directory.Exists(LanguageFolder) Then
            Process.Start(LanguageFolder)
        End If

    End Sub

    Dim searchResults As New List(Of String)
    Dim currentResult As Integer = 1

    Private Sub SearchBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles SearchBox.TextChanged

        searchResults.Clear()

        If SearchBox.Text = "" Then
            ResultsLabel.Visibility = Visibility.Collapsed
            PreviousResultButton.Visibility = Visibility.Collapsed
            NextResultButton.Visibility = Visibility.Collapsed
            ClearSearchBoxButton.Visibility = Visibility.Collapsed

        Else
            For Each item As String In OriginalStrings
                If item.ToLower.Contains(SearchBox.Text.ToLower) Then
                    searchResults.Add(OriginalStrings.IndexOf(item))

                End If

            Next

            ResultsLabel.Text = currentResult & "/" & searchResults.Count
            ResultsLabel.Visibility = Visibility.Visible
            PreviousResultButton.Visibility = Visibility.Visible
            NextResultButton.Visibility = Visibility.Visible
            ClearSearchBoxButton.Visibility = Visibility.Visible

            If searchResults.Count > 0 Then
                currentResult = 1
                GoToString(searchResults(0))
            Else
                currentResult = 0
                ResultsLabel.Text = currentResult & "/" & searchResults.Count
            End If
        End If

    End Sub

    Private Sub PreviousResult()
        If currentResult > 1 Then
            currentResult -= 1
            ResultsLabel.Text = currentResult & "/" & searchResults.Count

            GoToString(searchResults(currentResult - 1))
        End If
    End Sub

    Private Sub NextResult()
        If currentResult < searchResults.Count Then
            currentResult += 1
            ResultsLabel.Text = currentResult & "/" & searchResults.Count

            GoToString(searchResults(currentResult - 1))
        End If
    End Sub

    Private Sub NextResult_Click(sender As Object, e As RoutedEventArgs) Handles NextResultButton.Click
        NextResult()

    End Sub

    Private Sub PreviousResult_Click(sender As Object, e As RoutedEventArgs) Handles PreviousResultButton.Click
        PreviousResult()

    End Sub

    Private Sub ClearSearchBoxButton_Click(sender As Object, e As RoutedEventArgs) Handles ClearSearchBoxButton.Click
        SearchBox.Text = ""
    End Sub

    Private Sub SearchBox_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles SearchBox.PreviewKeyDown
        If Keyboard.IsKeyDown(Key.LeftCtrl) OrElse Keyboard.IsKeyDown(Key.RightCtrl) Then
            If e.Key = Key.Up Then
                e.Handled = True
                NextResult()
            ElseIf e.Key = Key.Down Then
                e.Handled = True
                PreviousResult()

            End If

        End If

        If e.Key = Key.Enter Then
            PlaceTheCaret()

        End If

    End Sub

    Private Sub CheckUpdatesButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckUpdatesButton.Click
        ReportNoUpdate = True
        If Not LatestVersionChecker.IsBusy Then LatestVersionChecker.RunWorkerAsync()

    End Sub

    Private Sub HelpButton_Click(sender As Object, e As RoutedEventArgs) Handles HelpButton.Click
        Dim help As New Help
        help.ShowDialog()

    End Sub

End Class
