
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
    <title>Parnacon</title>
    <%
		
        Dim iMenus, iSubMenus
                
        Dim vetMenus(3, 1), vetSubMenus()
		
        vetMenus(0, 0) = "Cadastros Principais"
        Redim vetSubMenus(1, 1)
		  
        vetSubMenus(0, 0) = "Cadastro de Cliente"
        vetSubMenus(0, 1) = "clientes.aspx"	          
        
        vetSubMenus(1, 0) = "Cadastro de Usuários"
        vetSubMenus(1, 1) = "usuarios.aspx"
        
        vetMenus(0, 1) = vetSubMenus		


        vetMenus(1, 0) = "Criação"
		
        Redim vetSubMenus(1, 2)
		
        vetSubMenus(0, 0) = "Briefing"
        vetSubMenus(0, 1) = "briefing.aspx"
		
        vetSubMenus(1, 0) = "Workpack"
        vetSubMenus(1, 1) = "workback.aspx"
        		  				  

        vetMenus(1, 1) = vetSubMenus
		
        vetMenus(2, 0) = "Gestor de Cotas"
		
        Redim vetSubMenus(2, 2)		
	
        vetSubMenus(1, 0) = "Im&oacute;veis"
        vetSubMenus(1, 1) = "gestor_cotas_imoveis.aspx"

        vetSubMenus(0, 0) = "Ve&iacute;culos"
        vetSubMenus(0, 1) = "gestor_cotas_veiculos.aspx"
		
        vetSubMenus(2, 0) = "Precifica&ccedil;&atilde;o"
        vetSubMenus(2, 1) = "precificacao.aspx"
		          
		
        vetMenus(2, 1) = vetSubMenus
		
        vetMenus(3, 0) = "Gestor (compra/venda)"
        
        Redim vetSubMenus(1, 2)		

        
        vetSubMenus(0, 0) = "Interessados (comprar)"
        vetSubMenus(0, 1) = "interessados_comprar.aspx"

        vetSubMenus(1, 0) = "Interessados (Vender)"
        vetSubMenus(1, 1) = "interessados_vender.aspx" 
          
        vetMenus(3, 1) = vetSubMenus


        vetMenus(4, 0) = "Sistema"

        Redim vetSubMenus(3, 2)
        
        vetSubMenus(0, 0) = "Alterar Senha"
        vetSubMenus(0, 1) = "alterar_senha.aspx"

        vetSubMenus(1, 0) = "Contatos"
        vetSubMenus(1, 1) = "contato.aspx"

        vetSubMenus(2, 0) = "Newsletter"
        vetSubMenus(2, 1) = "newsletter.aspx"

        vetSubMenus(3, 0) = "Sair"
        vetSubMenus(3, 1) = "logout.aspx"

        vetMenus(4, 1) = vetSubMenus                    
                	


    %>

    <meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1">
    <meta name="CODE_LANGUAGE" content="Visual Basic .NET 7.1">
    <meta name="vs_defaultClientScript" content="JavaScript">
    <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
    <link rel="stylesheet" href="Content/menu.css" type="text/css">
    <script type="text/javascript">

        function Ocultar() {
            document.getElementById('menu').style.display = "none";
            document.getElementById('seta').innerHTML = "<strong>&raquo;</strong>"

            document.getElementById('seta').onmousedown = Apresentar;

            window.parent.document.getElementById('estrutura2').cols = '20, *';

            return;
        }

        function Apresentar() {
            window.parent.document.getElementById('estrutura2').cols = '200, *';

            document.getElementById('menu').style.display = "block";
            document.getElementById('seta').innerHTML = "<strong>&laquo;</strong>"

            seta.onmousedown = Ocultar;

            return;
        }

        var intMenuSel = 0;

        function ClickMenu(oMenu, oSubMenu) {

            oSubMenu = document.getElementById(oSubMenu);

            if (oSubMenu.style.display != '') {
                oSubMenu.style.display = '';
            }
            else {
                oSubMenu.style.display = 'none';
            }

            return (false);
        }

    </script>
</head>
<body>
    <table style="background-color: #f1f1f1; height: 100%;" cellpadding="0" cellspacing="0">
        <tr>
            <td valign="top" id="celula_menu">
                <ul id="menu">
                    <% For iMenus = 0 To UBound(vetMenus) %>
                    <li class="ItemMenu">
                        <a href="#" onclick="javascript:ClickMenu(this, 'SubMenu<%=iMenus%>')" style="font-weight: bold;">
                            <%=vetMenus(iMenus, 0)%>
                        </a>
                        <ul class="SubMenu" id="SubMenu<%=iMenus%>" style="display: none">
                            <% For iSubMenus = 0 To UBound(vetMenus(iMenus, 1)) %>
                            <% strTarget = CStr(vetMenus(iMenus, 1)(iSubMenus, 2)) %>
                            <%	If strTarget = "" Then %>
                            <%		strTarget = "principal" %>
                            <%	End If %>
                            <li>
                                <a href="<%=vetMenus(iMenus, 1)(iSubMenus, 1)%>" target="<%=strTarget%>">
                                    <%=vetMenus(iMenus, 1)(iSubMenus, 0)%>
                                </a>
                                <% Next %>
                            </li>
                        </ul>
                        <% Next %>
                    </li>
                </ul>
            </td>
            <td id="celula_seta">
                <a href="#" onclick="javascript:Ocultar()" id="seta"><strong>&laquo;</strong></a>
            </td>
        </tr>
    </table>
</body>
</html>
