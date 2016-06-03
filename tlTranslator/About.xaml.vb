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

Imports System.Deployment.Application

Public Class About

    Public Sub New(ByVal versionNumber As String)

        InitializeComponent()

        VersionNumberLabel.Text = versionNumber
    End Sub

    Private Sub SourceCodeLink_Click(sender As Object, e As RoutedEventArgs) Handles SourceCodeLink.Click
        Process.Start("https://github.com/vistaero/TardisPluginLanguageFileTranslator")

    End Sub

    Private Sub KeyboardIconsRefer_Click(sender As Object, e As RoutedEventArgs) Handles KeyboardIconsRefer.Click
        Process.Start("http://www.iconarchive.com/show/keyboard-keys-icons-by-chromatix.2.html")
    End Sub

    Private Sub ContactVistaeroLink_Click(sender As Object, e As RoutedEventArgs) Handles ContactVistaeroLink.Click
        Process.Start("http://telegram.me/vistaero")
    End Sub
End Class
