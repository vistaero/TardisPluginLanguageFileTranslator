Public Class NewVersionInfo

    Private Sub NotNowButton_Click(sender As Object, e As RoutedEventArgs) Handles NotNowButton.Click
        Me.Close()

    End Sub

    Private Sub UpdateButton_Click(sender As Object, e As RoutedEventArgs) Handles UpdateButton.Click
        Process.Start("https://www.dropbox.com/s/50bexfnfj7qz35o/tlTranslator.zip?dl=1")
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub
End Class
