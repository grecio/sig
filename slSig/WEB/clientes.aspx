<%@ Page Title="" Language="C#" MasterPageFile="~/Painel.Master" AutoEventWireup="true" CodeBehind="clientes.aspx.cs" Inherits="Web.clientes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <table cellspacing="1" style="width: 92%">
        <tr>
            <td>
                <asp:Label ID="Label1" runat="server" CssClass="tit" Text="Clientes"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="linha">&nbsp;</td>
        </tr>
        <tr>
            <td>
                <asp:GridView ID="grdList" runat="server" AllowPaging="True" Width="100%" AutoGenerateColumns="False" BackColor="White" BorderColor="#3366CC" BorderStyle="None" BorderWidth="1px" CellPadding="4" CssClass="Grid" DataKeyNames="ID" EmptyDataText="Nenhum registro encontrado." OnPageIndexChanging="grdList_PageIndexChanging" OnRowDeleting="grdList_RowDeleting" OnSelectedIndexChanged="grdList_SelectedIndexChanged">
                    <AlternatingRowStyle CssClass="GridAltItem" />
                    <Columns>
                        <asp:BoundField DataField="idcliente" HeaderText="Cód" SortExpression="ID" InsertVisible="False" ReadOnly="True">
                            <HeaderStyle HorizontalAlign="Center" Width="5%" />
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:BoundField>
                        <asp:BoundField DataField="nome" HeaderText="Nome" SortExpression="Nome">
                            <HeaderStyle HorizontalAlign="Left" />
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:BoundField DataField="email" HeaderText="E-Mail" SortExpression="Email">
                            <HeaderStyle HorizontalAlign="Left" />
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:BoundField DataField="cpfcnpj" HeaderText="CPF" SortExpression="CPF">
                            <HeaderStyle HorizontalAlign="Center" />
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:BoundField>
                        <asp:BoundField DataField="telefone" HeaderText="Telefone">
                        <HeaderStyle Width="10%" />
                        <ItemStyle HorizontalAlign="Center" Width="10%" />
                        </asp:BoundField>
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Select" Text="Editar"></asp:LinkButton>
                            </ItemTemplate>
                            <HeaderStyle Width="7%" HorizontalAlign="Center" />
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Delete" Text="Remover"></asp:LinkButton>
                            </ItemTemplate>
                            <HeaderStyle Width="7%" HorizontalAlign="Center" />
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                    <FooterStyle BackColor="#99CCCC" ForeColor="#003399" />
                    <HeaderStyle BackColor="#1E2F5E" CssClass="GridHeader" Font-Bold="True" ForeColor="#CCCCFF" />
                    <PagerStyle BackColor="#99CCCC" ForeColor="#003399" HorizontalAlign="Left" />
                    <RowStyle BackColor="White" CssClass="GridItem" ForeColor="#003399" />
                    <SelectedRowStyle BackColor="#009999" Font-Bold="True" ForeColor="#CCFF99" />
                    <SortedAscendingCellStyle BackColor="#EDF6F6" />
                    <SortedAscendingHeaderStyle BackColor="#0D4AC4" />
                    <SortedDescendingCellStyle BackColor="#D6DFDF" />
                    <SortedDescendingHeaderStyle BackColor="#002876" />
                </asp:GridView>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="Label4" runat="server" CssClass="tit" Text="Clientes  - Formulário"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="linha">&nbsp;</td>
        </tr>
        <tr>
            <td>
                <table>
                    <tr>
                        <td>
                            <asp:Label ID="Label2" runat="server" CssClass="lbl" Text="Nome:" Width="100px"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtNome" runat="server" CssClass="txt" Width="555px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="Label5" runat="server" CssClass="lbl" Text="Email:" Width="100px"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="txt" Width="555px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="Label6" runat="server" CssClass="lbl" Text="CPF:"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtCPF" runat="server" CssClass="txt" Width="200px" data-mask="integer"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="Label13" runat="server" CssClass="lbl" Text="Telefone:"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtCPF0" runat="server" CssClass="txt" Width="200px" data-mask="integer"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>
                            <asp:Button ID="btnSalvar" runat="server" CssClass="btn" Text="Salvar" OnClick="btnSalvar_Click" />
                            &nbsp;<asp:Button ID="btnCancelar" runat="server" CssClass="btn" Text="Cancelar" OnClick="btnCancelar_Click" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

</asp:Content>
