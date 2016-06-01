Class MainWindow

    Private MessageBox As New Xceed.Wpf.Toolkit.MessageBox

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

            Try
                OriginalString.Text = OriginalStrings(0)
                TranslatedString.Text = TranslatedStrings(0)

            Catch ex As Exception
                MessageBox.Show("File not valid", "Error", MessageBoxButton.OK)


            End Try

            NextButton.IsEnabled = True
            PreviousButton.IsEnabled = True
            SaveButton.IsEnabled = True
            CurrentStringLabel.IsEnabled = True
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
            If e.Key = Key.Up Then
                GoNext()
            ElseIf e.Key = Key.Down Then
                GoPrevious()
            ElseIf e.Key = Key.S AndAlso (Keyboard.Modifiers And ModifierKeys.Control) = ModifierKeys.Control Then
                Save()
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

    Private Sub CurrentStringLabel_KeyDown(sender As Object, e As KeyEventArgs) Handles CurrentStringLabel.KeyDown
        If e.Key = Key.Enter Then
            TranslatedString.Focus()

            PlaceTheCaret()
        End If


    End Sub

    Private Sub GoToString(ByVal StringNumber As Integer)
        If StringNumber>=0 and StringNumber <= OriginalStrings.Count Then
            CurrentString = StringNumber
            OriginalString.Text = OriginalStrings(CurrentString)
            TranslatedString.Text = TranslatedStrings(CurrentString)
        End If

    End Sub

    Private Sub GoNext()
        If CurrentString <> OriginalStrings.Count - 1 Then
            TranslatedStrings(CurrentString) = TranslatedString.Text

            CurrentString += 1
            OriginalString.Text = OriginalStrings(CurrentString)

            TranslatedString.Text = TranslatedStrings(CurrentString)
            CurrentStringLabel.Text = CurrentString

            PlaceTheCaret()
        End If
    End Sub

    Private Sub GoPrevious()
        If CurrentString <> 0 Then
            TranslatedStrings(CurrentString) = TranslatedString.Text

            CurrentString -= 1
            OriginalString.Text = OriginalStrings(CurrentString)

            TranslatedString.Text = TranslatedStrings(CurrentString)

            CurrentStringLabel.Text = CurrentString

            PlaceTheCaret()
        End If
    End Sub

    Private Sub PlaceTheCaret()
        Dim split As String() = TranslatedString.Text.Split(New [Char]() {":"c})
        For Each s As String In split
            TranslatedString.CaretIndex = s.Length + 3
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
        MessageBox.Show("Made by vistaero. You can contact me via Telegram. @vistaero" & vbNewLine & vbNewLine & "Source code: https://github.com/vistaero/TardisPluginLanguageFileTranslator", "About", MessageBoxButton.OK, MessageBoxImage.Information)

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


End Class
