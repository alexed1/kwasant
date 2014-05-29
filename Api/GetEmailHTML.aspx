<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GetEmailHTML.aspx.cs" Inherits="KwasantWeb.Api.GetEmailHTML" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%=GetEmailSubject()%></title>    
    <style type="text/css">
        body
{
    font-family:Verdana;
}
.displayLabel
{
    color: #6d6e70;
    width: 25%;
}
.heading
{
    color: #003c6a;
    font-size: 16px;
    font-weight: bold;
}
.subHeading
{
    color: #6d6e70;
    font-size: 16px;
    margin-left: 3px;
    margin-bottom: 4px;
}
.searchInput
{
    margin-top:2px; 
    width:45%; 
    border:none; 
    background-color:#E9E9E9; 
    color: #58595b;
    padding-left:2px; 
    padding-bottom:1px;
}
        
#emailContent
{
    padding:10px;
    background-color:#F3F3F3;
}
            
.info
{            
    /* outline radius for mozilla/firefox only */
    -moz-box-shadow:0 0 10px #000;
    -webkit-box-shadow:0 0 10px #000;
	
    box-shadow: #000 0px 0px 10px;
    padding:7px;
}

.navigation
{
    margin-left:4px;
    margin-right:4px;
    margin-top:10px;
}

.closeIcon
{
    position:relative;
    top:3px;
}

img
{
    border:none;
}
    </style>
</head>
<body>
    <form id="form1" runat="server" style="width: 400px;">
    <div class="info" style="height:100%;">
        <div id="emailSubject" class="subHeading">
            <%=GetEmailSubject()%>
        </div>
        <hr />
        <div>
            <table width="100%">
                <tr>
                    <td class="displayLabel">To:</td>
                    <td><%=GetTo()%></td>
                </tr>
                <tr>
                    <td class="displayLabel">CC:</td>
                    <td><%=GetCC()%></td>
                </tr>
                <tr>
                    <td class="displayLabel">BCC:</td>
                    <td><%=GetBCC()%></td>
                </tr>
                <tr>
                    <td class="displayLabel">Email:</td>
                    <td><%=GetEmail()%></td>
                </tr>
                <tr>
                    <td class="displayLabel">From:</td>
                    <td><%=GetFromPerson()%></td>
                </tr>
                <tr>
                    <td class="displayLabel">Attachments:</td>
                    <td><%=GetAttachments()%></td>
                </tr>
            </table>
        </div>
        <hr />
        <div id="emailContent">
            <%=GetContent()%>
        </div>
        <br />
    </div>
    </form>
</body>
</html>
