// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;

/// <summary>Defines Windows message identifiers.</summary>
public enum WindowsMessages : uint
{
    /// <summary>The WM_NULL value.</summary>
    WM_NULL = 0U,

    /// <summary>The WM_CREATE value.</summary>
    WM_CREATE = 1U,

    /// <summary>The WM_DESTROY value.</summary>
    WM_DESTROY = 2U,

    /// <summary>The WM_MOVE value.</summary>
    WM_MOVE = 3U,

    /// <summary>The WM_SIZE value.</summary>
    WM_SIZE = 5U,

    /// <summary>The WM_ACTIVATE value.</summary>
    WM_ACTIVATE = 6U,

    /// <summary>The WM_SETFOCUS value.</summary>
    WM_SETFOCUS = 7U,

    /// <summary>The WM_KILLFOCUS value.</summary>
    WM_KILLFOCUS = 8U,

    /// <summary>The WM_ENABLE value.</summary>
    WM_ENABLE = 10U,

    /// <summary>The WM_SETREDRAW value.</summary>
    WM_SETREDRAW = 11U,

    /// <summary>The WM_SETTEXT value.</summary>
    WM_SETTEXT = 12U,

    /// <summary>The WM_GETTEXT value.</summary>
    WM_GETTEXT = 13U,

    /// <summary>The WM_GETTEXTLENGTH value.</summary>
    WM_GETTEXTLENGTH = 14U,

    /// <summary>The WM_PAINT value.</summary>
    WM_PAINT = 15U,

    /// <summary>The WM_CLOSE value.</summary>
    WM_CLOSE = 16U,

    /// <summary>The WM_QUERYENDSESSION value.</summary>
    WM_QUERYENDSESSION = 17U,

    /// <summary>The WM_QUIT value.</summary>
    WM_QUIT = 18U,

    /// <summary>The WM_QUERYOPEN value.</summary>
    WM_QUERYOPEN = 19U,

    /// <summary>The WM_ERASEBKGND value.</summary>
    WM_ERASEBKGND = 20U,

    /// <summary>The WM_SYSCOLORCHANGE value.</summary>
    WM_SYSCOLORCHANGE = 21U,

    /// <summary>The WM_ENDSESSION value.</summary>
    WM_ENDSESSION = 22U,

    /// <summary>The WM_SHOWWINDOW value.</summary>
    WM_SHOWWINDOW = 24U,

    /// <summary>The WM_WININICHANGE value.</summary>
    WM_WININICHANGE = 26U,

    /// <summary>The WM_SETTINGCHANGE value.</summary>
    WM_SETTINGCHANGE = WM_WININICHANGE,

    /// <summary>The WM_DEVMODECHANGE value.</summary>
    WM_DEVMODECHANGE = 27U,

    /// <summary>The WM_ACTIVATEAPP value.</summary>
    WM_ACTIVATEAPP = 28U,

    /// <summary>The WM_FONTCHANGE value.</summary>
    WM_FONTCHANGE = 29U,

    /// <summary>The WM_TIMECHANGE value.</summary>
    WM_TIMECHANGE = 30U,

    /// <summary>The WM_CANCELMODE value.</summary>
    WM_CANCELMODE = 31U,

    /// <summary>The WM_SETCURSOR value.</summary>
    WM_SETCURSOR = 32U,

    /// <summary>The WM_MOUSEACTIVATE value.</summary>
    WM_MOUSEACTIVATE = 33U,

    /// <summary>The WM_CHILDACTIVATE value.</summary>
    WM_CHILDACTIVATE = 34U,

    /// <summary>The WM_QUEUESYNC value.</summary>
    WM_QUEUESYNC = 35U,

    /// <summary>The WM_GETMINMAXINFO value.</summary>
    WM_GETMINMAXINFO = 36U,

    /// <summary>The WM_PAINTICON value.</summary>
    WM_PAINTICON = 38U,

    /// <summary>The WM_ICONERASEBKGND value.</summary>
    WM_ICONERASEBKGND = 39U,

    /// <summary>The WM_NEXTDLGCTL value.</summary>
    WM_NEXTDLGCTL = 40U,

    /// <summary>The WM_SPOOLERSTATUS value.</summary>
    WM_SPOOLERSTATUS = 42U,

    /// <summary>The WM_DRAWITEM value.</summary>
    WM_DRAWITEM = 43U,

    /// <summary>The WM_MEASUREITEM value.</summary>
    WM_MEASUREITEM = 44U,

    /// <summary>The WM_DELETEITEM value.</summary>
    WM_DELETEITEM = 45U,

    /// <summary>The WM_VKEYTOITEM value.</summary>
    WM_VKEYTOITEM = 46U,

    /// <summary>The WM_CHARTOITEM value.</summary>
    WM_CHARTOITEM = 47U,

    /// <summary>The WM_SETFONT value.</summary>
    WM_SETFONT = 48U,

    /// <summary>The WM_GETFONT value.</summary>
    WM_GETFONT = 49U,

    /// <summary>The WM_SETHOTKEY value.</summary>
    WM_SETHOTKEY = 50U,

    /// <summary>The WM_GETHOTKEY value.</summary>
    WM_GETHOTKEY = 51U,

    /// <summary>The WM_QUERYDRAGICON value.</summary>
    WM_QUERYDRAGICON = 55U,

    /// <summary>The WM_COMPAREITEM value.</summary>
    WM_COMPAREITEM = 57U,

    /// <summary>The WM_GETOBJECT value.</summary>
    WM_GETOBJECT = 61U,

    /// <summary>The WM_COMPACTING value.</summary>
    WM_COMPACTING = 65U,

    /// <summary>The WM_COMMNOTIFY value.</summary>
    WM_COMMNOTIFY = 68U,

    /// <summary>The WM_WINDOWPOSCHANGING value.</summary>
    WM_WINDOWPOSCHANGING = 70U,

    /// <summary>The WM_WINDOWPOSCHANGED value.</summary>
    WM_WINDOWPOSCHANGED = 71U,

    /// <summary>The WM_POWER value.</summary>
    WM_POWER = 72U,

    /// <summary>The WM_COPYDATA value.</summary>
    WM_COPYDATA = 74U,

    /// <summary>The WM_CANCELJOURNAL value.</summary>
    WM_CANCELJOURNAL = 75U,

    /// <summary>The WM_NOTIFY value.</summary>
    WM_NOTIFY = 78U,

    /// <summary>The WM_INPUTLANGCHANGEREQUEST value.</summary>
    WM_INPUTLANGCHANGEREQUEST = 80U,

    /// <summary>The WM_INPUTLANGCHANGE value.</summary>
    WM_INPUTLANGCHANGE = 81U,

    /// <summary>The WM_TCARD value.</summary>
    WM_TCARD = 82U,

    /// <summary>The WM_HELP value.</summary>
    WM_HELP = 83U,

    /// <summary>The WM_USERCHANGED value.</summary>
    WM_USERCHANGED = 84U,

    /// <summary>The WM_NOTIFYFORMAT value.</summary>
    WM_NOTIFYFORMAT = 85U,

    /// <summary>The WM_CONTEXTMENU value.</summary>
    WM_CONTEXTMENU = 123U,

    /// <summary>The WM_STYLECHANGING value.</summary>
    WM_STYLECHANGING = 124U,

    /// <summary>The WM_STYLECHANGED value.</summary>
    WM_STYLECHANGED = 125U,

    /// <summary>The WM_DISPLAYCHANGE value.</summary>
    WM_DISPLAYCHANGE = 126U,

    /// <summary>The WM_GETICON value.</summary>
    WM_GETICON = 127U,

    /// <summary>The WM_SETICON value.</summary>
    WM_SETICON = 128U,

    /// <summary>The WM_NCCREATE value.</summary>
    WM_NCCREATE = 129U,

    /// <summary>The WM_NCDESTROY value.</summary>
    WM_NCDESTROY = 130U,

    /// <summary>The WM_NCCALCSIZE value.</summary>
    WM_NCCALCSIZE = 131U,

    /// <summary>The WM_NCHITTEST value.</summary>
    WM_NCHITTEST = 132U,

    /// <summary>The WM_NCPAINT value.</summary>
    WM_NCPAINT = 133U,

    /// <summary>The WM_NCACTIVATE value.</summary>
    WM_NCACTIVATE = 134U,

    /// <summary>The WM_GETDLGCODE value.</summary>
    WM_GETDLGCODE = 135U,

    /// <summary>The WM_SYNCPAINT value.</summary>
    WM_SYNCPAINT = 136U,

    /// <summary>The WM_SYNCTASK value.</summary>
    WM_SYNCTASK = 137U,

    /// <summary>The WM_KLUDGEMINRECT value.</summary>
    WM_KLUDGEMINRECT = 139U,

    /// <summary>The WM_LPKDRAWSWITCHWND value.</summary>
    WM_LPKDRAWSWITCHWND = 140U,

    /// <summary>The WM_UAHDESTROYWINDOW value.</summary>
    WM_UAHDESTROYWINDOW = 144U,

    /// <summary>The WM_UAHDRAWMENU value.</summary>
    WM_UAHDRAWMENU = 145U,

    /// <summary>The WM_UAHDRAWMENUITEM value.</summary>
    WM_UAHDRAWMENUITEM = 146U,

    /// <summary>The WM_UAHINITMENU value.</summary>
    WM_UAHINITMENU = 147U,

    /// <summary>The WM_UAHMEASUREMENUITEM value.</summary>
    WM_UAHMEASUREMENUITEM = 148U,

    /// <summary>The WM_UAHNCPAINTMENUPOPUP value.</summary>
    WM_UAHNCPAINTMENUPOPUP = 149U,

    /// <summary>The WM_UAHUPDATE value.</summary>
    WM_UAHUPDATE = 150U,

    /// <summary>The WM_NCMOUSEMOVE value.</summary>
    WM_NCMOUSEMOVE = 160U,

    /// <summary>The WM_NCLBUTTONDOWN value.</summary>
    WM_NCLBUTTONDOWN = 161U,

    /// <summary>The WM_NCLBUTTONUP value.</summary>
    WM_NCLBUTTONUP = 162U,

    /// <summary>The WM_NCLBUTTONDBLCLK value.</summary>
    WM_NCLBUTTONDBLCLK = 163U,

    /// <summary>The WM_NCRBUTTONDOWN value.</summary>
    WM_NCRBUTTONDOWN = 164U,

    /// <summary>The WM_NCRBUTTONUP value.</summary>
    WM_NCRBUTTONUP = 165U,

    /// <summary>The WM_NCRBUTTONDBLCLK value.</summary>
    WM_NCRBUTTONDBLCLK = 166U,

    /// <summary>The WM_NCMBUTTONDOWN value.</summary>
    WM_NCMBUTTONDOWN = 167U,

    /// <summary>The WM_NCMBUTTONUP value.</summary>
    WM_NCMBUTTONUP = 168U,

    /// <summary>The WM_NCMBUTTONDBLCLK value.</summary>
    WM_NCMBUTTONDBLCLK = 169U,

    /// <summary>The WM_NCXBUTTONDOWN value.</summary>
    WM_NCXBUTTONDOWN = 171U,

    /// <summary>The WM_NCXBUTTONUP value.</summary>
    WM_NCXBUTTONUP = 172U,

    /// <summary>The WM_NCXBUTTONDBLCLK value.</summary>
    WM_NCXBUTTONDBLCLK = 173U,

    /// <summary>The WM_NCUAHDRAWCAPTION value.</summary>
    WM_NCUAHDRAWCAPTION = 174U,

    /// <summary>The WM_NCUAHDRAWFRAME value.</summary>
    WM_NCUAHDRAWFRAME = 175U,

    /// <summary>The EM_GETSEL value.</summary>
    EM_GETSEL = 176U,

    /// <summary>The EM_SETSEL value.</summary>
    EM_SETSEL = 177U,

    /// <summary>The EM_GETRECT value.</summary>
    EM_GETRECT = 178U,

    /// <summary>The EM_SETRECT value.</summary>
    EM_SETRECT = 179U,

    /// <summary>The EM_SETRECTNP value.</summary>
    EM_SETRECTNP = 180U,

    /// <summary>The EM_SCROLL value.</summary>
    EM_SCROLL = 181U,

    /// <summary>The EM_LINESCROLL value.</summary>
    EM_LINESCROLL = 182U,

    /// <summary>The EM_SCROLLCARET value.</summary>
    EM_SCROLLCARET = 183U,

    /// <summary>The EM_GETMODIFY value.</summary>
    EM_GETMODIFY = 184U,

    /// <summary>The EM_SETMODIFY value.</summary>
    EM_SETMODIFY = 185U,

    /// <summary>The EM_GETLINECOUNT value.</summary>
    EM_GETLINECOUNT = 186U,

    /// <summary>The EM_LINEINDEX value.</summary>
    EM_LINEINDEX = 187U,

    /// <summary>The EM_SETHANDLE value.</summary>
    EM_SETHANDLE = 188U,

    /// <summary>The EM_GETHANDLE value.</summary>
    EM_GETHANDLE = 189U,

    /// <summary>The EM_GETTHUMB value.</summary>
    EM_GETTHUMB = 190U,

    /// <summary>The EM_LINELENGTH value.</summary>
    EM_LINELENGTH = 193U,

    /// <summary>The EM_REPLACESEL value.</summary>
    EM_REPLACESEL = 194U,

    /// <summary>The EM_SETFONT value.</summary>
    EM_SETFONT = 195U,

    /// <summary>The EM_GETLINE value.</summary>
    EM_GETLINE = 196U,

    /// <summary>The EM_LIMITTEXT value.</summary>
    EM_LIMITTEXT = 197U,

    /// <summary>The EM_CANUNDO value.</summary>
    EM_CANUNDO = 198U,

    /// <summary>The EM_UNDO value.</summary>
    EM_UNDO = 199U,

    /// <summary>The EM_FMTLINES value.</summary>
    EM_FMTLINES = 200U,

    /// <summary>The EM_LINEFROMCHAR value.</summary>
    EM_LINEFROMCHAR = 201U,

    /// <summary>The EM_SETWORDBREAK value.</summary>
    EM_SETWORDBREAK = 202U,

    /// <summary>The EM_SETTABSTOPS value.</summary>
    EM_SETTABSTOPS = 203U,

    /// <summary>The EM_SETPASSWORDCHAR value.</summary>
    EM_SETPASSWORDCHAR = 204U,

    /// <summary>The EM_EMPTYUNDOBUFFER value.</summary>
    EM_EMPTYUNDOBUFFER = 205U,

    /// <summary>The EM_GETFIRSTVISIBLELINE value.</summary>
    EM_GETFIRSTVISIBLELINE = 206U,

    /// <summary>The EM_SETREADONLY value.</summary>
    EM_SETREADONLY = 207U,

    /// <summary>The EM_SETWORDBREAKPROC value.</summary>
    EM_SETWORDBREAKPROC = 208U,

    /// <summary>The EM_GETWORDBREAKPROC value.</summary>
    EM_GETWORDBREAKPROC = 209U,

    /// <summary>The EM_GETPASSWORDCHAR value.</summary>
    EM_GETPASSWORDCHAR = 210U,

    /// <summary>The EM_SETMARGINS value.</summary>
    EM_SETMARGINS = 211U,

    /// <summary>The EM_GETMARGINS value.</summary>
    EM_GETMARGINS = 212U,

    /// <summary>The EM_GETLIMITTEXT value.</summary>
    EM_GETLIMITTEXT = 213U,

    /// <summary>The EM_POSFROMCHAR value.</summary>
    EM_POSFROMCHAR = 214U,

    /// <summary>The EM_CHARFROMPOS value.</summary>
    EM_CHARFROMPOS = 215U,

    /// <summary>The EM_SETIMESTATUS value.</summary>
    EM_SETIMESTATUS = 216U,

    /// <summary>The EM_GETIMESTATUS value.</summary>
    EM_GETIMESTATUS = 217U,

    /// <summary>The EM_MSGMAX value.</summary>
    EM_MSGMAX = 218U,

    /// <summary>The WM_INPUT_DEVICE_CHANGE value.</summary>
    WM_INPUT_DEVICE_CHANGE = 254U,

    /// <summary>The WM_INPUT value.</summary>
    WM_INPUT = 255U,

    /// <summary>The WM_KEYDOWN value.</summary>
    WM_KEYDOWN = 256U,

    /// <summary>The WM_KEYUP value.</summary>
    WM_KEYUP = 257U,

    /// <summary>The WM_CHAR value.</summary>
    WM_CHAR = 258U,

    /// <summary>The WM_DEADCHAR value.</summary>
    WM_DEADCHAR = 259U,

    /// <summary>The WM_SYSKEYDOWN value.</summary>
    WM_SYSKEYDOWN = 260U,

    /// <summary>The WM_SYSKEYUP value.</summary>
    WM_SYSKEYUP = 261U,

    /// <summary>The WM_SYSCHAR value.</summary>
    WM_SYSCHAR = 262U,

    /// <summary>The WM_SYSDEADCHAR value.</summary>
    WM_SYSDEADCHAR = 263U,

    /// <summary>The WM_UNICHAR value.</summary>
    WM_UNICHAR = 265U,

    /// <summary>The WM_IME_STARTCOMPOSITION value.</summary>
    WM_IME_STARTCOMPOSITION = 269U,

    /// <summary>The WM_IME_ENDCOMPOSITION value.</summary>
    WM_IME_ENDCOMPOSITION = 270U,

    /// <summary>The WM_IME_COMPOSITION value.</summary>
    WM_IME_COMPOSITION = 271U,

    /// <summary>The WM_INITDIALOG value.</summary>
    WM_INITDIALOG = 272U,

    /// <summary>The WM_COMMAND value.</summary>
    WM_COMMAND = 273U,

    /// <summary>The WM_SYSCOMMAND value.</summary>
    WM_SYSCOMMAND = 274U,

    /// <summary>The WM_TIMER value.</summary>
    WM_TIMER = 275U,

    /// <summary>The WM_HSCROLL value.</summary>
    WM_HSCROLL = 276U,

    /// <summary>The WM_VSCROLL value.</summary>
    WM_VSCROLL = 277U,

    /// <summary>The WM_INITMENU value.</summary>
    WM_INITMENU = 278U,

    /// <summary>The WM_INITMENUPOPUP value.</summary>
    WM_INITMENUPOPUP = 279U,

    /// <summary>The WM_MENUSELECT value.</summary>
    WM_MENUSELECT = 287U,

    /// <summary>The WM_MENUCHAR value.</summary>
    WM_MENUCHAR = 288U,

    /// <summary>The WM_ENTERIDLE value.</summary>
    WM_ENTERIDLE = 289U,

    /// <summary>The WM_MENURBUTTONUP value.</summary>
    WM_MENURBUTTONUP = 290U,

    /// <summary>The WM_MENUDRAG value.</summary>
    WM_MENUDRAG = 291U,

    /// <summary>The WM_MENUGETOBJECT value.</summary>
    WM_MENUGETOBJECT = 292U,

    /// <summary>The WM_UNINITMENUPOPUP value.</summary>
    WM_UNINITMENUPOPUP = 293U,

    /// <summary>The WM_MENUCOMMAND value.</summary>
    WM_MENUCOMMAND = 294U,

    /// <summary>The WM_CHANGEUISTATE value.</summary>
    WM_CHANGEUISTATE = 295U,

    /// <summary>The WM_UPDATEUISTATE value.</summary>
    WM_UPDATEUISTATE = 296U,

    /// <summary>The WM_QUERYUISTATE value.</summary>
    WM_QUERYUISTATE = 297U,

    /// <summary>The WM_CTLCOLORMSGBOX value.</summary>
    WM_CTLCOLORMSGBOX = 306U,

    /// <summary>The WM_CTLCOLOREDIT value.</summary>
    WM_CTLCOLOREDIT = 307U,

    /// <summary>The WM_CTLCOLORLISTBOX value.</summary>
    WM_CTLCOLORLISTBOX = 308U,

    /// <summary>The WM_CTLCOLORBTN value.</summary>
    WM_CTLCOLORBTN = 309U,

    /// <summary>The WM_CTLCOLORDLG value.</summary>
    WM_CTLCOLORDLG = 310U,

    /// <summary>The WM_CTLCOLORSCROLLBAR value.</summary>
    WM_CTLCOLORSCROLLBAR = 311U,

    /// <summary>The WM_CTLCOLORSTATIC value.</summary>
    WM_CTLCOLORSTATIC = 312U,

    /// <summary>The CB_GETEDITSEL value.</summary>
    CB_GETEDITSEL = 320U,

    /// <summary>The CB_LIMITTEXT value.</summary>
    CB_LIMITTEXT = 321U,

    /// <summary>The CB_SETEDITSEL value.</summary>
    CB_SETEDITSEL = 322U,

    /// <summary>The CB_ADDSTRING value.</summary>
    CB_ADDSTRING = 323U,

    /// <summary>The CB_DELETESTRING value.</summary>
    CB_DELETESTRING = 324U,

    /// <summary>The CB_DIR value.</summary>
    CB_DIR = 325U,

    /// <summary>The CB_GETCOUNT value.</summary>
    CB_GETCOUNT = 326U,

    /// <summary>The CB_GETCURSEL value.</summary>
    CB_GETCURSEL = 327U,

    /// <summary>The CB_GETLBTEXT value.</summary>
    CB_GETLBTEXT = 328U,

    /// <summary>The CB_GETLBTEXTLEN value.</summary>
    CB_GETLBTEXTLEN = 329U,

    /// <summary>The CB_INSERTSTRING value.</summary>
    CB_INSERTSTRING = 330U,

    /// <summary>The CB_RESETCONTENT value.</summary>
    CB_RESETCONTENT = 331U,

    /// <summary>The CB_FINDSTRING value.</summary>
    CB_FINDSTRING = 332U,

    /// <summary>The CB_SELECTSTRING value.</summary>
    CB_SELECTSTRING = 333U,

    /// <summary>The CB_SETCURSEL value.</summary>
    CB_SETCURSEL = 334U,

    /// <summary>The CB_SHOWDROPDOWN value.</summary>
    CB_SHOWDROPDOWN = 335U,

    /// <summary>The CB_GETITEMDATA value.</summary>
    CB_GETITEMDATA = 336U,

    /// <summary>The CB_SETITEMDATA value.</summary>
    CB_SETITEMDATA = 337U,

    /// <summary>The CB_GETDROPPEDCONTROLRECT value.</summary>
    CB_GETDROPPEDCONTROLRECT = 338U,

    /// <summary>The CB_SETITEMHEIGHT value.</summary>
    CB_SETITEMHEIGHT = 339U,

    /// <summary>The CB_GETITEMHEIGHT value.</summary>
    CB_GETITEMHEIGHT = 340U,

    /// <summary>The CB_SETEXTENDEDUI value.</summary>
    CB_SETEXTENDEDUI = 341U,

    /// <summary>The CB_GETEXTENDEDUI value.</summary>
    CB_GETEXTENDEDUI = 342U,

    /// <summary>The CB_GETDROPPEDSTATE value.</summary>
    CB_GETDROPPEDSTATE = 343U,

    /// <summary>The CB_FINDSTRINGEXACT value.</summary>
    CB_FINDSTRINGEXACT = 344U,

    /// <summary>The CB_SETLOCALE value.</summary>
    CB_SETLOCALE = 345U,

    /// <summary>The CB_GETLOCALE value.</summary>
    CB_GETLOCALE = 346U,

    /// <summary>The CB_GETTOPINDEX value.</summary>
    CB_GETTOPINDEX = 347U,

    /// <summary>The CB_SETTOPINDEX value.</summary>
    CB_SETTOPINDEX = 348U,

    /// <summary>The CB_GETHORIZONTALEXTENT value.</summary>
    CB_GETHORIZONTALEXTENT = 349U,

    /// <summary>The CB_SETHORIZONTALEXTENT value.</summary>
    CB_SETHORIZONTALEXTENT = 350U,

    /// <summary>The CB_GETDROPPEDWIDTH value.</summary>
    CB_GETDROPPEDWIDTH = 351U,

    /// <summary>The CB_SETDROPPEDWIDTH value.</summary>
    CB_SETDROPPEDWIDTH = 352U,

    /// <summary>The CB_INITSTORAGE value.</summary>
    CB_INITSTORAGE = 353U,

    /// <summary>The CB_MSGMAX_OLD value.</summary>
    CB_MSGMAX_OLD = 354U,

    /// <summary>The CB_MULTIPLEADDSTRING value.</summary>
    CB_MULTIPLEADDSTRING = 355U,

    /// <summary>The CB_GETCOMBOBOXINFO value.</summary>
    CB_GETCOMBOBOXINFO = 356U,

    /// <summary>The CB_MSGMAX value.</summary>
    CB_MSGMAX = 357U,

    /// <summary>The LB_ADDSTRING value.</summary>
    LB_ADDSTRING = 384U,

    /// <summary>The LB_INSERTSTRING value.</summary>
    LB_INSERTSTRING = 385U,

    /// <summary>The LB_DELETESTRING value.</summary>
    LB_DELETESTRING = 386U,

    /// <summary>The LB_SELITEMRANGEEX value.</summary>
    LB_SELITEMRANGEEX = 387U,

    /// <summary>The LB_RESETCONTENT value.</summary>
    LB_RESETCONTENT = 388U,

    /// <summary>The LB_SETSEL value.</summary>
    LB_SETSEL = 389U,

    /// <summary>The LB_SETCURSEL value.</summary>
    LB_SETCURSEL = 390U,

    /// <summary>The LB_GETSEL value.</summary>
    LB_GETSEL = 391U,

    /// <summary>The LB_GETCURSEL value.</summary>
    LB_GETCURSEL = 392U,

    /// <summary>The LB_GETTEXT value.</summary>
    LB_GETTEXT = 393U,

    /// <summary>The LB_GETTEXTLEN value.</summary>
    LB_GETTEXTLEN = 394U,

    /// <summary>The LB_GETCOUNT value.</summary>
    LB_GETCOUNT = 395U,

    /// <summary>The LB_SELECTSTRING value.</summary>
    LB_SELECTSTRING = 396U,

    /// <summary>The LB_DIR value.</summary>
    LB_DIR = 397U,

    /// <summary>The LB_GETTOPINDEX value.</summary>
    LB_GETTOPINDEX = 398U,

    /// <summary>The LB_FINDSTRING value.</summary>
    LB_FINDSTRING = 399U,

    /// <summary>The LB_GETSELCOUNT value.</summary>
    LB_GETSELCOUNT = 400U,

    /// <summary>The LB_GETSELITEMS value.</summary>
    LB_GETSELITEMS = 401U,

    /// <summary>The LB_SETTABSTOPS value.</summary>
    LB_SETTABSTOPS = 402U,

    /// <summary>The LB_GETHORIZONTALEXTENT value.</summary>
    LB_GETHORIZONTALEXTENT = 403U,

    /// <summary>The LB_SETHORIZONTALEXTENT value.</summary>
    LB_SETHORIZONTALEXTENT = 404U,

    /// <summary>The LB_SETCOLUMNWIDTH value.</summary>
    LB_SETCOLUMNWIDTH = 405U,

    /// <summary>The LB_ADDFILE value.</summary>
    LB_ADDFILE = 406U,

    /// <summary>The LB_SETTOPINDEX value.</summary>
    LB_SETTOPINDEX = 407U,

    /// <summary>The LB_GETITEMRECT value.</summary>
    LB_GETITEMRECT = 408U,

    /// <summary>The LB_GETITEMDATA value.</summary>
    LB_GETITEMDATA = 409U,

    /// <summary>The LB_SETITEMDATA value.</summary>
    LB_SETITEMDATA = 410U,

    /// <summary>The LB_SELITEMRANGE value.</summary>
    LB_SELITEMRANGE = 411U,

    /// <summary>The LB_SETANCHORINDEX value.</summary>
    LB_SETANCHORINDEX = 412U,

    /// <summary>The LB_GETANCHORINDEX value.</summary>
    LB_GETANCHORINDEX = 413U,

    /// <summary>The LB_SETCARETINDEX value.</summary>
    LB_SETCARETINDEX = 414U,

    /// <summary>The LB_GETCARETINDEX value.</summary>
    LB_GETCARETINDEX = 415U,

    /// <summary>The LB_SETITEMHEIGHT value.</summary>
    LB_SETITEMHEIGHT = 416U,

    /// <summary>The LB_GETITEMHEIGHT value.</summary>
    LB_GETITEMHEIGHT = 417U,

    /// <summary>The LB_FINDSTRINGEXACT value.</summary>
    LB_FINDSTRINGEXACT = 418U,

    /// <summary>The LBCB_CARETON value.</summary>
    LBCB_CARETON = 419U,

    /// <summary>The LBCB_CARETOFF value.</summary>
    LBCB_CARETOFF = 420U,

    /// <summary>The LB_SETLOCALE value.</summary>
    LB_SETLOCALE = 421U,

    /// <summary>The LB_GETLOCALE value.</summary>
    LB_GETLOCALE = 422U,

    /// <summary>The LB_SETCOUNT value.</summary>
    LB_SETCOUNT = 423U,

    /// <summary>The LB_INITSTORAGE value.</summary>
    LB_INITSTORAGE = 424U,

    /// <summary>The LB_ITEMFROMPOINT value.</summary>
    LB_ITEMFROMPOINT = 425U,

    /// <summary>The LB_INSERTSTRINGUPPER value.</summary>
    LB_INSERTSTRINGUPPER = 426U,

    /// <summary>The LB_INSERTSTRINGLOWER value.</summary>
    LB_INSERTSTRINGLOWER = 427U,

    /// <summary>The LB_ADDSTRINGUPPER value.</summary>
    LB_ADDSTRINGUPPER = 428U,

    /// <summary>The LB_ADDSTRINGLOWER value.</summary>
    LB_ADDSTRINGLOWER = 429U,

    /// <summary>The LBCB_STARTTRACK value.</summary>
    LBCB_STARTTRACK = 430U,

    /// <summary>The LBCB_ENDTRACK value.</summary>
    LBCB_ENDTRACK = 431U,

    /// <summary>The LB_MSGMAX_OLD value.</summary>
    LB_MSGMAX_OLD = 432U,

    /// <summary>The LB_MULTIPLEADDSTRING value.</summary>
    LB_MULTIPLEADDSTRING = 433U,

    /// <summary>The LB_GETLISTBOXINFO value.</summary>
    LB_GETLISTBOXINFO = 434U,

    /// <summary>The LB_MSGMAX value.</summary>
    LB_MSGMAX = 435U,

    /// <summary>The MN_FIRST value.</summary>
    MN_FIRST = 480U,

    /// <summary>The WM_GETHMENU value.</summary>
    WM_GETHMENU = 481U,

    /// <summary>The WM_MOUSEMOVE value.</summary>
    WM_MOUSEMOVE = 512U,

    /// <summary>The WM_LBUTTONDOWN value.</summary>
    WM_LBUTTONDOWN = 513U,

    /// <summary>The WM_LBUTTONUP value.</summary>
    WM_LBUTTONUP = 514U,

    /// <summary>The WM_LBUTTONDBLCLK value.</summary>
    WM_LBUTTONDBLCLK = 515U,

    /// <summary>The WM_RBUTTONDOWN value.</summary>
    WM_RBUTTONDOWN = 516U,

    /// <summary>The WM_RBUTTONUP value.</summary>
    WM_RBUTTONUP = 517U,

    /// <summary>The WM_RBUTTONDBLCLK value.</summary>
    WM_RBUTTONDBLCLK = 518U,

    /// <summary>The WM_MBUTTONDOWN value.</summary>
    WM_MBUTTONDOWN = 519U,

    /// <summary>The WM_MBUTTONUP value.</summary>
    WM_MBUTTONUP = 520U,

    /// <summary>The WM_MBUTTONDBLCLK value.</summary>
    WM_MBUTTONDBLCLK = 521U,

    /// <summary>The WM_MOUSEWHEEL value.</summary>
    WM_MOUSEWHEEL = 522U,

    /// <summary>The WM_XBUTTONDOWN value.</summary>
    WM_XBUTTONDOWN = 523U,

    /// <summary>The WM_XBUTTONUP value.</summary>
    WM_XBUTTONUP = 524U,

    /// <summary>The WM_XBUTTONDBLCLK value.</summary>
    WM_XBUTTONDBLCLK = 525U,

    /// <summary>The WM_MOUSEHWHEEL value.</summary>
    WM_MOUSEHWHEEL = 526U,

    /// <summary>The WM_PARENTNOTIFY value.</summary>
    WM_PARENTNOTIFY = 528U,

    /// <summary>The WM_ENTERMENULOOP value.</summary>
    WM_ENTERMENULOOP = 529U,

    /// <summary>The WM_EXITMENULOOP value.</summary>
    WM_EXITMENULOOP = 530U,

    /// <summary>The WM_NEXTMENU value.</summary>
    WM_NEXTMENU = 531U,

    /// <summary>The WM_SIZING value.</summary>
    WM_SIZING = 532U,

    /// <summary>The WM_CAPTURECHANGED value.</summary>
    WM_CAPTURECHANGED = 533U,

    /// <summary>The WM_MOVING value.</summary>
    WM_MOVING = 534U,

    /// <summary>The WM_POWERBROADCAST value.</summary>
    WM_POWERBROADCAST = 536U,

    /// <summary>The WM_DEVICECHANGE value.</summary>
    WM_DEVICECHANGE = 537U,

    /// <summary>The WM_MDICREATE value.</summary>
    WM_MDICREATE = 544U,

    /// <summary>The WM_MDIDESTROY value.</summary>
    WM_MDIDESTROY = 545U,

    /// <summary>The WM_MDIACTIVATE value.</summary>
    WM_MDIACTIVATE = 546U,

    /// <summary>The WM_MDIRESTORE value.</summary>
    WM_MDIRESTORE = 547U,

    /// <summary>The WM_MDINEXT value.</summary>
    WM_MDINEXT = 548U,

    /// <summary>The WM_MDIMAXIMIZE value.</summary>
    WM_MDIMAXIMIZE = 549U,

    /// <summary>The WM_MDITILE value.</summary>
    WM_MDITILE = 550U,

    /// <summary>The WM_MDICASCADE value.</summary>
    WM_MDICASCADE = 551U,

    /// <summary>The WM_MDIICONARRANGE value.</summary>
    WM_MDIICONARRANGE = 552U,

    /// <summary>The WM_MDIGETACTIVE value.</summary>
    WM_MDIGETACTIVE = 553U,

    /// <summary>The WM_MDISETMENU value.</summary>
    WM_MDISETMENU = 560U,

    /// <summary>The WM_ENTERSIZEMOVE value.</summary>
    WM_ENTERSIZEMOVE = 561U,

    /// <summary>The WM_EXITSIZEMOVE value.</summary>
    WM_EXITSIZEMOVE = 562U,

    /// <summary>The WM_DROPFILES value.</summary>
    WM_DROPFILES = 563U,

    /// <summary>The WM_MDIREFRESHMENU value.</summary>
    WM_MDIREFRESHMENU = 564U,

    /// <summary>The WM_IME_REPORT value.</summary>
    WM_IME_REPORT = 640U,

    /// <summary>The WM_IME_SETCONTEXT value.</summary>
    WM_IME_SETCONTEXT = 641U,

    /// <summary>The WM_IME_NOTIFY value.</summary>
    WM_IME_NOTIFY = 642U,

    /// <summary>The WM_IME_CONTROL value.</summary>
    WM_IME_CONTROL = 643U,

    /// <summary>The WM_IME_COMPOSITIONFULL value.</summary>
    WM_IME_COMPOSITIONFULL = 644U,

    /// <summary>The WM_IME_SELECT value.</summary>
    WM_IME_SELECT = 645U,

    /// <summary>The WM_IME_CHAR value.</summary>
    WM_IME_CHAR = 646U,

    /// <summary>The WM_IME_REQUEST value.</summary>
    WM_IME_REQUEST = 648U,

    /// <summary>The WM_IME_KEYDOWN value.</summary>
    WM_IME_KEYDOWN = 656U,

    /// <summary>The WM_IME_KEYUP value.</summary>
    WM_IME_KEYUP = 657U,

    /// <summary>The WM_NCMOUSEHOVER value.</summary>
    WM_NCMOUSEHOVER = 672U,

    /// <summary>The WM_MOUSEHOVER value.</summary>
    WM_MOUSEHOVER = 673U,

    /// <summary>The WM_NCMOUSELEAVE value.</summary>
    WM_NCMOUSELEAVE = 674U,

    /// <summary>The WM_MOUSELEAVE value.</summary>
    WM_MOUSELEAVE = 675U,

    /// <summary>The WM_WTSSESSION_CHANGE value.</summary>
    WM_WTSSESSION_CHANGE = 689U,

    /// <summary>The WM_TABLET_FIRST value.</summary>
    WM_TABLET_FIRST = 704U,

    /// <summary>The WM_POINTERDEVICEADDED value.</summary>
    WM_POINTERDEVICEADDED = 712U,

    /// <summary>The WM_POINTERDEVICEDELETED value.</summary>
    WM_POINTERDEVICEDELETED = 713U,

    /// <summary>The WM_FLICK value.</summary>
    WM_FLICK = 715U,

    /// <summary>The WM_FLICKINTERNAL value.</summary>
    WM_FLICKINTERNAL = 717U,

    /// <summary>The WM_BRIGHTNESSCHANGED value.</summary>
    WM_BRIGHTNESSCHANGED = 718U,

    /// <summary>The WM_TABLET_LAST value.</summary>
    WM_TABLET_LAST = 735U,

    /// <summary>The WM_DPICHANGED value.</summary>
    WM_DPICHANGED = 736U,

    /// <summary>The WM_DPICHANGED_BEFOREPARENT value.</summary>
    WM_DPICHANGED_BEFOREPARENT = 738U,

    /// <summary>The WM_DPICHANGED_AFTERPARENT value.</summary>
    WM_DPICHANGED_AFTERPARENT = 739U,

    /// <summary>The WM_GETDPISCALEDSIZE value.</summary>
    WM_GETDPISCALEDSIZE = 740U,

    /// <summary>The WM_CUT value.</summary>
    WM_CUT = 768U,

    /// <summary>The WM_COPY value.</summary>
    WM_COPY = 769U,

    /// <summary>The WM_PASTE value.</summary>
    WM_PASTE = 770U,

    /// <summary>The WM_CLEAR value.</summary>
    WM_CLEAR = 771U,

    /// <summary>The WM_UNDO value.</summary>
    WM_UNDO = 772U,

    /// <summary>The WM_RENDERFORMAT value.</summary>
    WM_RENDERFORMAT = 773U,

    /// <summary>The WM_RENDERALLFORMATS value.</summary>
    WM_RENDERALLFORMATS = 774U,

    /// <summary>The WM_DESTROYCLIPBOARD value.</summary>
    WM_DESTROYCLIPBOARD = 775U,

    /// <summary>The WM_DRAWCLIPBOARD value.</summary>
    WM_DRAWCLIPBOARD = 776U,

    /// <summary>The WM_PAINTCLIPBOARD value.</summary>
    WM_PAINTCLIPBOARD = 777U,

    /// <summary>The WM_VSCROLLCLIPBOARD value.</summary>
    WM_VSCROLLCLIPBOARD = 778U,

    /// <summary>The WM_SIZECLIPBOARD value.</summary>
    WM_SIZECLIPBOARD = 779U,

    /// <summary>The WM_ASKCBFORMATNAME value.</summary>
    WM_ASKCBFORMATNAME = 780U,

    /// <summary>The WM_CHANGECBCHAIN value.</summary>
    WM_CHANGECBCHAIN = 781U,

    /// <summary>The WM_HSCROLLCLIPBOARD value.</summary>
    WM_HSCROLLCLIPBOARD = 782U,

    /// <summary>The WM_QUERYNEWPALETTE value.</summary>
    WM_QUERYNEWPALETTE = 783U,

    /// <summary>The WM_PALETTEISCHANGING value.</summary>
    WM_PALETTEISCHANGING = 784U,

    /// <summary>The WM_PALETTECHANGED value.</summary>
    WM_PALETTECHANGED = 785U,

    /// <summary>The WM_HOTKEY value.</summary>
    WM_HOTKEY = 786U,

    /// <summary>The WM_SYSMENU value.</summary>
    WM_SYSMENU = 787U,

    /// <summary>The WM_HOOKMSG value.</summary>
    WM_HOOKMSG = 788U,

    /// <summary>The WM_EXITPROCESS value.</summary>
    WM_EXITPROCESS = 789U,

    /// <summary>The WM_WAKETHREAD value.</summary>
    WM_WAKETHREAD = 790U,

    /// <summary>The WM_PRINT value.</summary>
    WM_PRINT = 791U,

    /// <summary>The WM_PRINTCLIENT value.</summary>
    WM_PRINTCLIENT = 792U,

    /// <summary>The WM_APPCOMMAND value.</summary>
    WM_APPCOMMAND = 793U,

    /// <summary>The WM_THEMECHANGED value.</summary>
    WM_THEMECHANGED = 794U,

    /// <summary>The WM_UAHINIT value.</summary>
    WM_UAHINIT = 795U,

    /// <summary>The WM_DESKTOPNOTIFY value.</summary>
    WM_DESKTOPNOTIFY = 796U,

    /// <summary>The WM_CLIPBOARDUPDATE value.</summary>
    WM_CLIPBOARDUPDATE = 797U,

    /// <summary>The WM_DWMCOMPOSITIONCHANGED value.</summary>
    WM_DWMCOMPOSITIONCHANGED = 798U,

    /// <summary>The WM_DWMNCRENDERINGCHANGED value.</summary>
    WM_DWMNCRENDERINGCHANGED = 799U,

    /// <summary>The WM_DWMCOLORIZATIONCOLORCHANGED value.</summary>
    WM_DWMCOLORIZATIONCOLORCHANGED = 800U,

    /// <summary>The WM_DWMWINDOWMAXIMIZEDCHANGE value.</summary>
    WM_DWMWINDOWMAXIMIZEDCHANGE = 801U,

    /// <summary>The WM_DWMEXILEFRAME value.</summary>
    WM_DWMEXILEFRAME = 802U,

    /// <summary>The WM_DWMSENDICONICTHUMBNAIL value.</summary>
    WM_DWMSENDICONICTHUMBNAIL = 803U,

    /// <summary>The WM_MAGNIFICATION_STARTED value.</summary>
    WM_MAGNIFICATION_STARTED = 804U,

    /// <summary>The WM_MAGNIFICATION_ENDED value.</summary>
    WM_MAGNIFICATION_ENDED = 805U,

    /// <summary>The WM_DWMSENDICONICLIVEPREVIEWBITMAP value.</summary>
    WM_DWMSENDICONICLIVEPREVIEWBITMAP = 806U,

    /// <summary>The WM_DWMTHUMBNAILSIZECHANGED value.</summary>
    WM_DWMTHUMBNAILSIZECHANGED = 807U,

    /// <summary>The WM_MAGNIFICATION_OUTPUT value.</summary>
    WM_MAGNIFICATION_OUTPUT = 808U,

    /// <summary>The WM_BSDRDATA value.</summary>
    WM_BSDRDATA = 809U,

    /// <summary>The WM_DWMTRANSITIONSTATECHANGED value.</summary>
    WM_DWMTRANSITIONSTATECHANGED = 810U,

    /// <summary>The WM_KEYBOARDCORRECTIONCALLOUT value.</summary>
    WM_KEYBOARDCORRECTIONCALLOUT = 812U,

    /// <summary>The WM_KEYBOARDCORRECTIONACTION value.</summary>
    WM_KEYBOARDCORRECTIONACTION = 813U,

    /// <summary>The WM_UIACTION value.</summary>
    WM_UIACTION = 814U,

    /// <summary>The WM_ROUTED_UI_EVENT value.</summary>
    WM_ROUTED_UI_EVENT = 815U,

    /// <summary>The WM_MEASURECONTROL value.</summary>
    WM_MEASURECONTROL = 816U,

    /// <summary>The WM_GETACTIONTEXT value.</summary>
    WM_GETACTIONTEXT = 817U,

    /// <summary>The WM_FORWARDKEYDOWN value.</summary>
    WM_FORWARDKEYDOWN = 819U,

    /// <summary>The WM_FORWARDKEYUP value.</summary>
    WM_FORWARDKEYUP = 820U,

    /// <summary>The WM_GETTITLEBARINFOEX value.</summary>
    WM_GETTITLEBARINFOEX = 831U,

    /// <summary>The WM_NOTIFYWOW value.</summary>
    WM_NOTIFYWOW = 832U,

    /// <summary>The WM_HANDHELDFIRST value.</summary>
    WM_HANDHELDFIRST = 856U,

    /// <summary>The WM_HANDHELDLAST value.</summary>
    WM_HANDHELDLAST = 863U,

    /// <summary>The WM_AFXFIRST value.</summary>
    WM_AFXFIRST = 864U,

    /// <summary>The WM_AFXLAST value.</summary>
    WM_AFXLAST = 895U,

    /// <summary>The WM_PENWINFIRST value.</summary>
    WM_PENWINFIRST = 896U,

    /// <summary>The WM_PENWINLAST value.</summary>
    WM_PENWINLAST = 911U,

    /// <summary>The MM_JOY1MOVE value.</summary>
    MM_JOY1MOVE = 928U,

    /// <summary>The MM_JOY2MOVE value.</summary>
    MM_JOY2MOVE = 929U,

    /// <summary>The MM_JOY1ZMOVE value.</summary>
    MM_JOY1ZMOVE = 930U,

    /// <summary>The MM_JOY2ZMOVE value.</summary>
    MM_JOY2ZMOVE = 931U,

    /// <summary>The MM_JOY1BUTTONDOWN value.</summary>
    MM_JOY1BUTTONDOWN = 949U,

    /// <summary>The MM_JOY2BUTTONDOWN value.</summary>
    MM_JOY2BUTTONDOWN = 950U,

    /// <summary>The MM_JOY1BUTTONUP value.</summary>
    MM_JOY1BUTTONUP = 951U,

    /// <summary>The MM_JOY2BUTTONUP value.</summary>
    MM_JOY2BUTTONUP = 952U,

    /// <summary>The MM_MCINOTIFY value.</summary>
    MM_MCINOTIFY = 953U,

    /// <summary>The MM_WOM_OPEN value.</summary>
    MM_WOM_OPEN = 955U,

    /// <summary>The MM_WOM_CLOSE value.</summary>
    MM_WOM_CLOSE = 956U,

    /// <summary>The MM_WOM_DONE value.</summary>
    MM_WOM_DONE = 957U,

    /// <summary>The MM_WIM_OPEN value.</summary>
    MM_WIM_OPEN = 958U,

    /// <summary>The MM_WIM_CLOSE value.</summary>
    MM_WIM_CLOSE = 959U,

    /// <summary>The MM_WIM_DATA value.</summary>
    MM_WIM_DATA = 960U,

    /// <summary>The MM_MIM_OPEN value.</summary>
    MM_MIM_OPEN = 961U,

    /// <summary>The MM_MIM_CLOSE value.</summary>
    MM_MIM_CLOSE = 962U,

    /// <summary>The MM_MIM_DATA value.</summary>
    MM_MIM_DATA = 963U,

    /// <summary>The MM_MIM_LONGDATA value.</summary>
    MM_MIM_LONGDATA = 964U,

    /// <summary>The MM_MIM_ERROR value.</summary>
    MM_MIM_ERROR = 965U,

    /// <summary>The MM_MIM_LONGERROR value.</summary>
    MM_MIM_LONGERROR = 966U,

    /// <summary>The MM_MOM_OPEN value.</summary>
    MM_MOM_OPEN = 967U,

    /// <summary>The MM_MOM_CLOSE value.</summary>
    MM_MOM_CLOSE = 968U,

    /// <summary>The MM_MOM_DONE value.</summary>
    MM_MOM_DONE = 969U,

    /// <summary>The MM_DRVM_OPEN value.</summary>
    MM_DRVM_OPEN = 976U,

    /// <summary>The MM_DRVM_CLOSE value.</summary>
    MM_DRVM_CLOSE = 977U,

    /// <summary>The MM_DRVM_DATA value.</summary>
    MM_DRVM_DATA = 978U,

    /// <summary>The MM_DRVM_ERROR value.</summary>
    MM_DRVM_ERROR = 979U,

    /// <summary>The MM_STREAM_OPEN value.</summary>
    MM_STREAM_OPEN = 980U,

    /// <summary>The MM_STREAM_CLOSE value.</summary>
    MM_STREAM_CLOSE = 981U,

    /// <summary>The MM_STREAM_DONE value.</summary>
    MM_STREAM_DONE = 982U,

    /// <summary>The MM_STREAM_ERROR value.</summary>
    MM_STREAM_ERROR = 983U,

    /// <summary>The MM_MOM_POSITIONCB value.</summary>
    MM_MOM_POSITIONCB = 970U,

    /// <summary>The MM_MCISIGNAL value.</summary>
    MM_MCISIGNAL = 971U,

    /// <summary>The MM_MIM_MOREDATA value.</summary>
    MM_MIM_MOREDATA = 972U,

    /// <summary>The WM_USER value.</summary>
    WM_USER = 1024U,

    /// <summary>The NIN_SELECT value.</summary>
    NIN_SELECT = WM_USER,

    /// <summary>The NIN_KEYSELECT value.</summary>
    NIN_KEYSELECT = 1025U,

    /// <summary>The NIN_BALLOONSHOW value.</summary>
    NIN_BALLOONSHOW = 1026U,

    /// <summary>The NIN_BALLOONHIDE value.</summary>
    NIN_BALLOONHIDE = 1027U,

    /// <summary>The NIN_BALLOONTIMEOUT value.</summary>
    NIN_BALLOONTIMEOUT = 1028U,

    /// <summary>The NIN_BALLOONUSERCLICK value.</summary>
    NIN_BALLOONUSERCLICK = 1029U,

    /// <summary>The NIN_POPUPOPEN value.</summary>
    NIN_POPUPOPEN = 1030U,

    /// <summary>The NIN_POPUPCLOSE value.</summary>
    NIN_POPUPCLOSE = 1031U,

    /// <summary>The WM_APP value.</summary>
    WM_APP = 32_768U,

    /// <summary>The WM_REFLECT value.</summary>
    WM_REFLECT = 8192U,

    /// <summary>The WM_APPLICATION_STRING value.</summary>
    WM_APPLICATION_STRING = 49_152U,

    /// <summary>The WM_RASDIALEVENT value.</summary>
    WM_RASDIALEVENT = 52_429U,
}
