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
    Private Sub About_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            VersionNumber.Text = "Version: " & ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString
        Catch ex As Exception
            VersionNumber.Text = "Debug Version"
        End Try

    End Sub

    Private Sub SourceCodeLink_Click(sender As Object, e As RoutedEventArgs) Handles SourceCodeLink.Click
        Process.Start("https://github.com/vistaero/TardisPluginLanguageFileTranslator")

    End Sub
End Class
