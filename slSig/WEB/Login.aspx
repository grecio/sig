<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Web.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Content/StyleSheet.css" rel="stylesheet" />
    </head>
<body>
    <form id="form1" runat="server">
        <div>
            <table border="0" cellpadding="0" cellspacing="0" width="100%">
                <tr>
                    <td align="center" valign="middle" style="background-image:url(imgs/topo.png); background-repeat:no-repeat; height:75px;"></td>
                </tr>
                <tr>
                    <td align="center" valign="middle">
                        <br />
                        <br />
                        <table border="0" cellpadding="1" cellspacing="1" style="border:1px solid #ccc; background-color: #fafafa;">
                            <tr>
                                <td align="center" class="lbl">O acesso a esta aplicação é restrito aos usuários autorizados. 
                                        <br />
                                    Por favor identifique-se.</td>
                            </tr>
                            <tr>
                                <td style="height: 20px">&nbsp;</td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <table border="0" cellpadding="1" cellspacing="1">
                                        <tr>
                                            <td> <asp:Label ID="Label1" runat="server" CssClass="lbl" Text="Login/e-mail:"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtLogin" runat="server" CssClass="txt" Width="200px" MaxLength="50"></asp:TextBox>
                                            </td>
                                        </tr>
                                        
                                        <tr>
                                            <td>
                                                 <asp:Label ID="Label2" runat="server" CssClass="lbl" Text="Senha:"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtSenha" runat="server" CssClass="txt" TextMode="Password" Width="200px"
                                                    MaxLength="20"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Button ID="btnEntrar" runat="server" Text="Entrar" CssClass="btn" OnClick="btnEntrar_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td align="center" valign="middle">
                        <asp:Label ID="lblMensagem" runat="server" CssClass="lblbold"></asp:Label>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
