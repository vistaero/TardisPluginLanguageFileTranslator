Class MainWindow

    Private MessageBox As New Xceed.Wpf.Toolkit.MessageBox

    Private FilePath As String = ""
    Private FileContent As Object

    Private SavePath As String

    Private OriginalStrings As New List(Of String)
    Private TranslatedStrings As New List(Of String)

    Private CurrentString As Integer = 0

    Private WarningText As String = "## do not remove or change '%s' as these are needed to insert names, commands and config options"
    Private TextToSave As String = vbNewLine & WarningText

    'I have no idea when I can set this to false... So is always false.
    Private Saved As Boolean = False

    Private Sub LoadButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadButton.Click
        LoadWindow()

    End Sub

    Private Sub LoadWindow()
        Dim OpenFileDialog As New Microsoft.Win32.OpenFileDialog
        OpenFileDialog.Filter = "YML|*.yml"
        OpenFileDialog.ShowDialog()

        If OpenFileDialog.FileName <> "" Then
            FilePath = OpenFileDialog.FileName
            Load()
            My.Settings.LastFilePath = FilePath
            My.Settings.Save()
        End If
    End Sub

    Private Sub Load()
        If FilePath <> "" Then
            FileContent = ""
            SavePath = ""
            OriginalStrings = New List(Of String)
            TranslatedStrings = New List(Of String)
            CurrentString = 0
            TextToSave = vbNewLine & WarningText
        End If

        Try
            FileContent = System.IO.File.ReadAllLines(FilePath)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            LoadWindow()
            Exit Sub

        End Try


        For Each Line In FileContent
            If Line <> "" And Not Line.ToString.StartsWith("#") Then
                OriginalStrings.Add(Line)
            End If
        Next

        For Each item As String In OriginalStrings
            TranslatedStrings.Add(item)
        Next

        Try
            OriginalString.Text = OriginalStrings(0)
            TranslatedString.Text = TranslatedStrings(0)

        Catch ex As Exception
            MessageBox.Show("File not valid", "Error", MessageBoxButton.OK)
            LoadWindow()

        End Try

        NextButton.IsEnabled = True
        PreviousButton.IsEnabled = True
        SaveAsButton.IsEnabled = True
        SaveButton.IsEnabled = True
        TotalStringsLabel.Content = OriginalStrings.Count - 1

        TranslatedString.Focus()
        PlaceTheCaret()

        FilePathLabel.Text = FilePath
        SavePath = FilePath

    End Sub

    Private Sub GoNext()
        If CurrentString <> OriginalStrings.Count - 1 Then
            TranslatedStrings(CurrentString) = TranslatedString.Text

            CurrentString += 1
            OriginalString.Text = OriginalStrings(CurrentString)

            TranslatedString.Text = TranslatedStrings(CurrentString)
            CurrentStringLabel.Content = CurrentString

            PlaceTheCaret()
        End If
    End Sub

    Private Sub GoPrevious()
        If CurrentString <> 0 Then
            TranslatedStrings(CurrentString) = TranslatedString.Text

            CurrentString -= 1
            OriginalString.Text = OriginalStrings(CurrentString)

            TranslatedString.Text = TranslatedStrings(CurrentString)

            CurrentStringLabel.Content = CurrentString

        End If
    End Sub

    Private Sub PlaceTheCaret()
        Dim split As String() = TranslatedString.Text.Split(New [Char]() {":"c})
        For Each s As String In split
            TranslatedString.CaretIndex = s.Length + 3
            Exit For

        Next
    End Sub

    Private Sub NextButton_Click(sender As Object, e As RoutedEventArgs) Handles NextButton.Click
        GoNext()
    End Sub

    Private Sub PreviousButton_Click(sender As Object, e As RoutedEventArgs) Handles PreviousButton.Click
        GoPrevious()
    End Sub

    Private Sub TranslatedString_KeyUp(sender As Object, e As KeyEventArgs) Handles TranslatedString.KeyUp
        If e.Key = Key.Down Then PlaceTheCaret()
    End Sub

    Private Sub TranslatedString_KeyDown(sender As Object, e As KeyEventArgs) Handles TranslatedString.PreviewKeyDown
        If FilePath <> "" Then
            If e.Key = Key.Up Then
                GoNext()
            ElseIf e.Key = Key.Down Then
                GoPrevious()
            ElseIf e.Key = Key.S AndAlso (Keyboard.Modifiers And ModifierKeys.Control) = ModifierKeys.Control Then
                Save()
            End If
        End If

    End Sub

    Private Sub GenerateText()
        TextToSave = ""
        TextToSave = vbNewLine & WarningText

        TranslatedStrings(CurrentString) = TranslatedString.Text

        For Each item In TranslatedStrings
            TextToSave += vbNewLine & item
        Next

        TextToSave += vbNewLine
    End Sub

    Private Sub SaveAsButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveAsButton.Click

        Dim SaveFileDialog As New Microsoft.Win32.SaveFileDialog
        SaveFileDialog.Filter = "YML|*.yml"
        If SaveFileDialog.ShowDialog() = True And SaveFileDialog.SafeFileName <> "" Then
            SavePath = SaveFileDialog.FileName
            FilePath = SavePath
            FilePathLabel.Text = FilePath
            Save()
        End If

    End Sub

    Private Sub Save()
        GenerateText()
        Try
            System.IO.File.WriteAllText(SavePath, TextToSave)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Save()
    End Sub

    Private Sub AboutButton_Click(sender As Object, e As RoutedEventArgs) Handles AboutButton.Click

        MessageBox.Show("Made by vistaero. You can contact me via Twitter. @vistaero" & vbNewLine & "Source code: https://github.com/vistaero/TardisPluginLanguageFileTranslator", "About", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        MessageBox.CaptionIcon = Nothing
        If My.Settings.LastFilePath <> "" And System.IO.File.Exists(My.Settings.LastFilePath) = True Then
            If MessageBox.Show("Do you want to load the last file you have open?", "", MessageBoxButton.YesNo) = MessageBoxResult.Yes Then
                FilePath = My.Settings.LastFilePath
                Load()
            Else
                My.Settings.LastFilePath = ""
                LoadWindow()

            End If
        Else
            LoadWindow()
            Saved = True
        End If

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If FilePath <> "" And Saved = False Then
            Dim result As MessageBoxResult = MessageBox.Show("Save before exit?", "", MessageBoxButton.YesNoCancel)
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


End Class
