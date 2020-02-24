﻿/*
	Include in output:

	This file is part of Natural Docs, which is Copyright © 2003-2020 Code Clear LLC.
	Natural Docs is licensed under version 3 of the GNU Affero General Public
	License (AGPL).  Refer to License.txt or www.naturaldocs.org for the
	complete details.

	This file may be distributed with documentation files generated by Natural Docs.
	Such documentation is not covered by Natural Docs' copyright and licensing,
	and may have its own copyright and distribution terms as decided by its author.

	This file includes code derived from jQuery HashChange Event, which is
	Copyright © 2010 "Cowboy" Ben Alman.  jQuery HashChange Event may be
	obtained separately under the MIT license or the GNU General Public License (GPL).
	However, this combined product is still licensed under the terms of the AGPLv3.

*/

"use strict";


/* Class: NDFramePage
	_____________________________________________________________________________

	Topic: URL Hash Format

		File References:

			> #File[number unless 1]:[full path]:[full symbol (optional)]
			>
			> #File:source/module/file.cs
			> #File3:source/module/file.cs:Namespace.Package.Function

		Path Restrictions:

			Because of the above format, generated file paths cannot contain the colon character.  This is
			documented in <CodeClear.NaturalDocs.Engine.Output.Builders.HTML.Path Restrictions>.

*/
var NDFramePage = new function ()
	{

	// Group: Functions
	// ________________________________________________________________________


	/* Function: Start
	*/
	this.Start = function ()
		{
		var ieVersion = NDCore.IEVersion();

		
		// The default title of the page is the project title.  Save a copy before we mess with it.
		
		this.projectTitle = document.title;


		// Transition the page from our static default loading screen to the active layout.

		var loadingNotice = document.getElementById("NDLoadingNotice");
		loadingNotice.parentNode.removeChild(loadingNotice);

		// True or false determines whether it will be made visible later.
		var pageElements = {
			NDHeader: true,
			NDSearchField: true,
			NDFooter: true,
			NDMenu: true, 
			NDSummary: false, // UpdateLayout() will enable this if necessary
			NDContent: true,
			NDMessages: false,
			NDMenuSizer: true, // Needs to be visible, but is styled as transparent unless hovered over
			NDSummarySizer: true // Needs to be visible, but is styled as transparent unless hovered over
			};
		var pageElementPositioning = "fixed";

		// IE 6 doesn't like fixed positioning the way other browsers do.  It works with absolute positioning though.
		if (ieVersion == 6)
			{  pageElementPositioning = "absolute";  }

		// Resizing is flaky on IE prior to 8, so just disable it.
		if (ieVersion < 8)
			{
			document.getElementById("NDMenuSizer").style.display = "none";
			delete pageElements.NDMenuSizer;

			document.getElementById("NDSummarySizer").style.display = "none";
			delete pageElements.NDSummarySizer;
			}

		// IE will sometimes put a disabled scrollbar on the right side of the window if this isn't done.  It isn't always
		// predictable though.  IE 7 will always do it in my virtual machine, but IE 6 and 8 won't.  However, IE 8 does
		// do it on a different computer even though they're both running the same version and are both XP.  Weird.
		// Since it shouldn't have any detrimental effect, add it for all IE versions just to be safe.
		if (ieVersion !== undefined)
			{  document.getElementsByTagName("html")[0].style.overflow = "hidden";  }

		// Done with the browser tweaks.  Update the layout.
		for (var pageElementName in pageElements)
			{
			var domElement = document.getElementById(pageElementName);
			domElement.style.position = pageElementPositioning;

			if (pageElements[pageElementName] == true)
				{  domElement.style.display = "block";  }
			}

		// this.desiredMenuWidth = undefined;
		// this.desiredSummaryWidth = undefined;

		this.UpdateLayout();


		// Attach event handlers

		window.onresize = function () {  NDFramePage.OnResize();  };
		document.onmousedown = function (e) {  return NDFramePage.OnMouseDown(e);  };
		this.AddHashChangeHandler();

		// We want to close the search results when we click anywhere else.  OnMouseDown handles clicks on most of the
		// page but not inside the content iframe.  window.onblur will fire for IE 10, Firefox, and Chrome.  document.onblur 
		// will only fire for Firefox.  IE 8 and earlier won't fire either, oh well.
		window.onblur = function () {  NDFramePage.OnBlur();  };


		// Start panels

		NDMenu.Start();
		NDSummary.Start();
		NDSearch.Start();


		// Load the hash location, if any.

		this.OnHashChange();
		};


	/* Function: Message
		Posts a message on the screen.
	*/
	this.Message = function (message)
		{
		var htmlEntry = document.createElement("div");
		htmlEntry.className = "MsgMessage";

		var htmlMessage = document.createTextNode(message);
		htmlEntry.appendChild(htmlMessage);

		document.getElementById("MsgContent").appendChild(htmlEntry);
		document.getElementById("NDMessages").style.display = "block";
		this.OnResize();
		};


	/* Function: CloseMessages
	*/
	this.CloseMessages = function ()
		{
		document.getElementById("NDMessages").style.display = "none";
		document.getElementById("MsgContent").innerHTML = "";
		};


	/* Function: OnBlur
	*/
	this.OnBlur = function ()
		{
		if (NDSearch.SearchFieldIsActive())
			{
			NDSearch.ClearResults();
			NDSearch.DeactivateSearchField();
			}
		};



	// Group: Hash and Navigation Functions
	// ________________________________________________________________________


	/* Function: OnHashChange
	*/
	this.OnHashChange = function ()
		{
		var oldLocation = this.currentLocation;
		this.currentLocation = new NDLocation(location.hash);


		// Update the poller for IE browsers that support the onhashchange event to prevent this function from
		// being called twice.  The poller will therefore only catch navigation that the event doesn't fire for,
		// like moving from "Member" to "member".

		if (this.hashChangePoller != undefined)
			{  this.hashChangePoller.lastHash = location.hash;  }

		
		// Clear any search results since that may be the way we got here.

		NDSearch.ClearResults();
		NDSearch.DeactivateSearchField();


		// We need to update the layout if the location changes the visibility of the summary panel.

		var oldLocationHasSummary = (oldLocation != undefined && oldLocation.summaryFile != undefined);
		var currentLocationHasSummary = (this.currentLocation.summaryFile != undefined);

		if (oldLocationHasSummary != currentLocationHasSummary)
			{  this.UpdateLayout();  }


		// Set the content page

		var frame = document.getElementById("CFrame");

		// Internet Explorer treats anchors as case-insensitive.  If the hash path has a member we won't do the navigation here
		// because it would confuse member and Member.  Instead we'll let the summary look up the member and do the navigation 
		// with a Topic# anchor.
		if (NDCore.IsIE() && this.currentLocation.type == "File" && this.currentLocation.member != undefined)
			{
			// Do nothing.  We can't even go to the base page here and let the summary replace it with the anchor because they don't
			// always occur in the right order.
			}
		else
			{  
			// Everything else is case sensitive and can go right to the target without waiting for the summary to load.
			frame.contentWindow.location.replace(this.currentLocation.contentPage);
			}


		// Set focus to the content page iframe so that keyboard scrolling works without clicking over to it.

		frame.contentWindow.focus();


		// Notify the panels

		NDMenu.OnLocationChange(oldLocation, this.currentLocation);
		NDSummary.OnLocationChange(oldLocation, this.currentLocation);


		// Normally the page title will be updated by the summary metadata file, but we have to do it manually if the new 
		// location won't have a summary, such as the home page.

		if (this.currentLocation.summaryFile == undefined)
			{  this.UpdatePageTitle();  }
		};


	/* Function: OnPageTitleLoaded
		Called by a source file's metadata when it's loaded.
	*/
	this.OnPageTitleLoaded = function (hashPath, title)
		{
		if (this.currentLocation.path == hashPath)
			{  this.UpdatePageTitle(title);  }
		};


	/* Function: UpdatePageTitle
	*/
	this.UpdatePageTitle = function (pageTitle)
		{
		if (pageTitle)
			{  document.title = pageTitle + " - " + this.projectTitle;  }
		else
			{  document.title = this.projectTitle;  }
		};


	/* Function: AddHashChangeHandler
		Sets up <OnHashChange()> to be called whenever a hash change event occurs.  Based on jQuery HashChange
		Event because not all browsers support window.onhashchange.
	*/
	this.AddHashChangeHandler = function ()
		{
		// Note that IE8 running in IE7 compatibility mode reports true for "onhashchange" in window even
		// though the event isn't supported, so also test document.documentMode.
		var supportsOnHashChange = ("onhashchange" in window && (document.documentMode === undefined || document.documentMode > 7));

		// Add a straightforward hash change handler if the browser supports it.
		if (supportsOnHashChange)
			{
			window.onhashchange = function () {  NDFramePage.OnHashChange();  };
			}

		// Add a poller if the browser doesn't, or is IE.  We need it for IE even when it supports onhashchange because it 
		// treats anchors as case-insensitive, so the event won't fire when navigating from "Member" to "member".
		if (!supportsOnHashChange || NDCore.IsIE())
			{
			this.hashChangePoller = {
				// timeoutID: undefined,
				timeoutLength: 200,  // Every fifth of a second

				// Remember the initial hash so it doesn't get triggered immediately.
				lastHash: location.hash
				};

			// Non-IE browsers that don't support onhashchange and IE versions that do can use a straightforward polling 
			// loop of the hash.
			if (!NDCore.IsIE() || supportsOnHashChange)
				{
				this.hashChangePoller.Start = function ()
					{
					this.Poll();
					};

				this.hashChangePoller.Stop = function ()
					{
					if (this.timeoutID != undefined)
						{
						clearTimeout(this.timeoutID);
						this.timeoutID = undefined;
						}
					};

				this.hashChangePoller.Poll = function ()
					{
					if (location.hash != this.lastHash)
						{
						this.lastHash = location.hash;
						NDFramePage.OnHashChange();
						}

					this.timeoutID = setTimeout("NDFramePage.hashChangePoller.Poll()", this.timeoutLength);
					};
				}

			else  // IE versions that don't support onhashchange
				{
				// Not only do IE6/7 need the "magical" iframe treatment, but so does IE8
				// when running in IE7 compatibility mode.

				this.hashChangePoller.Start = function ()
					{
					// Create a hidden iframe for history handling.
					var iframeElement = document.createElement("iframe");

					// Attempt to make it as hidden as possible by using techniques from
					// http://www.paciellogroup.com/blog/?p=604
					iframeElement.title = "empty";
					iframeElement.tabindex = -1;
					iframeElement.style.display = "none";
					iframeElement.width = 0;
					iframeElement.height = 0;
					iframeElement.src = "javascript:0";

					this.firstRun = true;

					iframeElement.attachEvent("onload",
						function ()
							{
							if (NDFramePage.hashChangePoller.firstRun)
								{
								NDFramePage.hashChangePoller.firstRun = false;
								NDFramePage.hashChangePoller.SetHistory(location.hash);

								NDFramePage.hashChangePoller.Poll();
								}
							}
						);

					// jQuery HashChange Event does some stuff I'm not 100% clear on to "append iframe after
					// the end of the body to prevent unnecessary initial page scrolling (yes, this works)."  Bah,
					// screw it, let's just go with straightforward.
					document.body.appendChild(iframeElement);

					this.iframe = iframeElement.contentWindow;

					// Whenever the document.title changes, update the iframe's title to
					// prettify the back/next history menu entries.  Since IE sometimes
					// errors with "Unspecified error" the very first time this is set
					// (yes, very useful) wrap this with a try/catch block.
					document.onpropertychange = function ()
						{
						if (event.propertyName == "title")
							{
							try
								{  NDFramePage.hashChangePoller.iframe.document.title = document.title;  }
							catch(e)
								{  }
							}
						};
					};

				// No Stop method since an IE6/7 iframe was created.  Even
				// without an event handler the polling loop would still be necessary
				// for back/next to work at all!
				this.hashChangePoller.Stop = function () { };


				this.hashChangePoller.Poll = function ()
					{
					var hash = location.hash;
					var historyHash = this.GetHistory();

					// If location.hash changed, which covers mouse clicks and manual editing
					if (hash != this.lastHash)
						{
						this.lastHash = location.hash;
						this.SetHistory(hash, historyHash);
						NDFramePage.OnHashChange();
						}

					// If historyHash changed, which covers back and forward buttons
					else if (historyHash != this.lastHash)
						{
						location.href = location.href.replace( /#.*/, '' ) + historyHash;
						}

					this.timeoutID = setTimeout("NDFramePage.hashChangePoller.Poll()", this.timeoutLength);
					};

				this.hashChangePoller.GetHistory = function ()
					{
					return this.iframe.location.hash;
					};

				this.hashChangePoller.SetHistory = function (hash, historyHash)
					{
					if (hash != historyHash)
						{
						// Update iframe with any initial document.title that might be set.
						this.iframe.document.title = document.title;

						// Opening the iframe's document after it has been closed is what
						// actually adds a history entry.
						this.iframe.document.open();

						this.iframe.document.close();

						// Update the iframe's hash, for great justice.
						this.iframe.location.hash = hash;
						}
					};
				}

			this.hashChangePoller.Start();
			}
		};



	// Group: Layout Functions
	// ________________________________________________________________________


	/* Function: OnResize
	*/
	this.OnResize = function ()
		{
		this.UpdateLayout();
		};


	/* Function: UpdateLayout
		Positions all elements on the page.
	*/
	this.UpdateLayout = function ()
		{
		var ieVersion = NDCore.IEVersion();
		var useSizers = (ieVersion == undefined || ieVersion >= 8);

		var fullWidth = NDCore.WindowClientWidth();
		var fullHeight = NDCore.WindowClientHeight();

		var header = document.getElementById("NDHeader");
		var searchField = document.getElementById("NDSearchField");
		var footer = document.getElementById("NDFooter");
		var menu = document.getElementById("NDMenu");
		var menuSizer = document.getElementById("NDMenuSizer");
		var summary = document.getElementById("NDSummary");
		var summarySizer = document.getElementById("NDSummarySizer");
		var content = document.getElementById("NDContent");
		var messages = document.getElementById("NDMessages");

		NDCore.SetToAbsolutePosition(header, 0, 0, fullWidth, undefined);
		NDCore.SetToAbsolutePosition(footer, 0, undefined, fullWidth, undefined);

		// Treat the header as one pixel shorter than it actually is.  This makes it so it there's a lip that sits under the 
		// rest of the page elements.  We do this because when browsers are set to zoom levels greater than 100%,
		// rounding errors may make 1px gaps appear between elements.  For most elements this isn't as issue because
		// the menu background color blends in.  The header is different because a gray bar between it and the home 
		// page is very noticable.
		var headerHeight = header.offsetHeight - 1;
		var footerHeight = footer.offsetHeight;

		// We needed separate calls to set the footer's Y position and width since wrapping may change its height.
		NDCore.SetToAbsolutePosition(footer, undefined, fullHeight - footerHeight, undefined, undefined);

		var searchMargin = (headerHeight - searchField.offsetHeight) / 2;
		NDCore.SetToAbsolutePosition(searchField, fullWidth - searchField.offsetWidth - searchMargin, searchMargin, undefined, undefined);

		var remainingHeight = fullHeight - headerHeight - footerHeight;
		var remainingWidth = fullWidth;
		var currentX = 0;

		// The order of operations below is very important.  Block has to be set before checking the offset width or it
		// might return zero.  It also has to be set before setting the position or Firefox will sometimes not show
		// scrollbars on the summary panel when navigating back and forth between the home page where it's hidden
		// and regular pages where it's not.

		if (this.MenuIsVisible())
			{
			menu.style.display = "block";
			NDCore.SetToAbsolutePosition(menu, currentX, headerHeight, undefined, remainingHeight);

			currentX += menu.offsetWidth;
			remainingWidth -= menu.offsetWidth;

			if (this.desiredMenuWidth == undefined)
				{  this.desiredMenuWidth = menu.offsetWidth;  }

			if (useSizers)
				{
				menuSizer.style.display = "block";
				NDCore.SetToAbsolutePosition(menuSizer, currentX, headerHeight, undefined, remainingHeight);
				}

			NDMenu.OnUpdateLayout();
			}
		else
			{
			menu.style.display = "none";
			menuSizer.style.display = "none";
			}

		if (this.SummaryIsVisible())
			{
			summary.style.display = "block";
			NDCore.SetToAbsolutePosition(summary, currentX, headerHeight, undefined, remainingHeight);

			currentX += summary.offsetWidth;
			remainingWidth -= summary.offsetWidth;

			if (this.desiredSummaryWidth == undefined)
				{  this.desiredSummaryWidth = summary.offsetWidth;  }

			if (useSizers)
				{
				summarySizer.style.display = "block";
				NDCore.SetToAbsolutePosition(summarySizer, currentX, headerHeight, undefined, remainingHeight);
				}
			}
		else
			{
			summary.style.display = "none";
			summarySizer.style.display = "none";
			}

		NDCore.SetToAbsolutePosition(content, currentX, headerHeight, remainingWidth, remainingHeight);
		NDCore.SetToAbsolutePosition(messages, currentX, 0, remainingWidth, undefined);
		NDSearch.OnUpdateLayout();
		};


	/* Function: MenuIsVisible
	*/
	this.MenuIsVisible = function ()
		{
		return true;
		};


	/* Function: SummaryIsVisible
	*/
	this.SummaryIsVisible = function ()
		{
		return (this.currentLocation != undefined && this.currentLocation.summaryFile != undefined);
		};


	/* Function: OnMouseDown
	*/
	this.OnMouseDown = function (event)
		{
		if (event == undefined)
			{  event = window.event;  }

		var target = event.target || event.srcElement;

		if (NDSearch.SearchFieldIsActive())
			{
			var targetIsInResults = false;

			for (var element = target; element != undefined; element = element.parentNode)
				{
				if (element.id == "NDSearchResults")
					{  
					targetIsInResults = true;
					break;
					}
				}

			if (!targetIsInResults)
				{
				NDSearch.ClearResults();
				NDSearch.DeactivateSearchField();
				}
			}

		if (target.id == "NDMenuSizer" || target.id == "NDSummarySizer")
			{
			var element;

			if (target.id == "NDMenuSizer")
				{  element = document.getElementById("NDMenu");  }
			else
				{  element = document.getElementById("NDSummary");  }

			this.sizerDragging =
				{
				"sizer": target,
				"element": element,
				"originalSizerX": target.offsetLeft,
				"originalElementWidth": element.offsetWidth,
				"originalClientX": event.clientX
				};

			NDCore.AddClass(target, "Dragging");

			document.onmousemove = function (e) {  return NDFramePage.OnSizerMouseMove(e);  };
			document.onmouseup = function (e) {  return NDFramePage.OnSizerMouseUp(e);  };
			document.onselectstart = function () {  return false;  };  // Helps IE

			// We need a div to cover the content iframe or else if you drag too fast over it IE will send some of the messages
			// to the iframe instead.  The z-index is set in CSS to be between the sizers and everything else.
			var contentCover = document.createElement("div");
			contentCover.id = "NDContentCover";

			// Must be appended before calling SetToAbsolutePosition or it won't position properly.
			document.body.appendChild(contentCover);

			NDCore.SetToAbsolutePosition(contentCover, 0, 0, NDCore.WindowClientWidth(), NDCore.WindowClientHeight());

			return false;
			}
		else
			{  return true;  }
		};


	/* Function: OnSizerMouseMove
	*/
	this.OnSizerMouseMove = function (event)
		{
		if (event == undefined)
			{  event = window.event;  }

		var offset = event.clientX - this.sizerDragging.originalClientX;
		var windowClientWidth = NDCore.WindowClientWidth();

		// Sanity checks
		if (this.sizerDragging.sizer.id == "NDMenuSizer")
			{
			if (this.sizerDragging.originalSizerX + offset < 0)
				{  offset = 0 - this.sizerDragging.originalSizerX;  }
			else if (this.sizerDragging.originalSizerX + offset + this.sizerDragging.sizer.offsetWidth > windowClientWidth)
				{  offset = windowClientWidth - this.sizerDragging.sizer.offsetWidth - this.sizerDragging.originalSizerX;  }
			}
		else // "NDSummarySizer"
			{
			var menuSizer = document.getElementById("NDMenuSizer");
			var leftLimit = menuSizer.offsetLeft + menuSizer.offsetWidth;

			if (this.sizerDragging.originalSizerX + offset < leftLimit)
				{  offset = leftLimit - this.sizerDragging.originalSizerX;  }
			else if (this.sizerDragging.originalSizerX + offset + this.sizerDragging.sizer.offsetWidth > windowClientWidth)
				{  offset = windowClientWidth - this.sizerDragging.sizer.offsetWidth - this.sizerDragging.originalSizerX;  }
			}

		NDCore.SetToAbsolutePosition(this.sizerDragging.sizer, this.sizerDragging.originalSizerX + offset, undefined, undefined, undefined);
		NDCore.SetToAbsolutePosition(this.sizerDragging.element, undefined, undefined, this.sizerDragging.originalElementWidth + offset, undefined);

		if (this.sizerDragging.sizer.id == "NDMenuSizer")
			{  this.desiredMenuWidth = document.getElementById("NDMenu").offsetWidth;  }
		else // "NDSummarySizer
			{  this.desiredSummaryWidth = document.getElementById("NDSummary").offsetWidth;  }

		this.UpdateLayout();
		};


	/* Function: OnSizerMouseUp
	*/
	this.OnSizerMouseUp = function (event)
		{
		// Doesn't work if you use undefined.  Must be null.
		document.onmousemove = null;
		document.onmouseup = null;
		document.onselectstart = null;

		document.body.removeChild(document.getElementById("NDContentCover"));

		NDCore.RemoveClass(this.sizerDragging.sizer, "Dragging");
		this.sizerDragging = undefined;
		};


	/* Function: SizeSummaryToContent
		Resizes the summary panel to try to show its content without a horizontal scrollbar.  The new width will have a 
		minimum of <desiredSummaryWidth> and a maximum of <desiredSummaryWidth> times <$ExpansionFactor>.  This 
		is to be called by <NDSummary> whenever it's content changes.
	*/
	this.SizeSummaryToContent = function ()
		{
		this.SizePanelToContent(document.getElementById("NDSummary"), this.desiredSummaryWidth);
		};


	// I decided not to implement similar functionality for NDMenu, though it can be supported just as easily.  Just create
	// SizeMenuToContent() and call it from NDMenu.Update().


	/* Function: SizePanelToContent
		Resizes the passed panel to try to show its content without a horizontal scrollbar.  The new width will have a
		minimum of desiredOffsetWidth and a maximum of desiredOffsetWidth times <$ExpansionFactor>.
	*/
	this.SizePanelToContent = function (panel, desiredOffsetWidth)
		{
		// For reference:
		//    clientWidth/Height - Size of visible content area
		//    offsetWidth/Height - Size of visible content area plus scrollbars and borders
		//    scrollWidth/Height - Size of content

		// This may happen the first time the panel is loaded if it happens before the first UpdateLayout().
		if (this.desiredSummaryWidth == undefined)
			{  return;  }

		var resized = false;

		// If there's no horizontal scroll bar... 
		// (scrollWidth will never be less than clientWidth, even if the content doesn't need all the room.)
		if (panel.clientWidth == panel.scrollWidth)
			{  
			// and we're already at the desired width, there's nothing to do.
			if (panel.offsetWidth == desiredOffsetWidth)
				{  return;  }
			else
				{
				// The panel is different than the desired width, meaning it was automatically expanded for the previous
				// content.  There's no way for us to determine the minimum content width to only shrink it down when
				// necessary, so we have to reset it and then determine if we need to expand it again.
				NDCore.SetToAbsolutePosition(panel, undefined, undefined, desiredOffsetWidth, undefined);
				resized = true;
				}
			}
		// else
			// If there is a horizontal scrollbar, that means scrollWidth is set to the minimum content width and we can
			// continue regardless of whether the panel is the desired size.

		var newOffsetWidth = panel.scrollWidth;

		// Do we have a vertical scroll bar?
		if (panel.scrollHeight > panel.clientHeight)
			{  
			// If so factor it in.  offset - client will include the left and right border widths too.
			newOffsetWidth += panel.offsetWidth - panel.clientWidth;  
			}
		else
			{
			// If not just factor in the border widths.  This only works if they're specified in px in the CSS.
			newOffsetWidth += NDCore.GetComputedPixelWidth(panel, "borderLeftWidth") +
										 NDCore.GetComputedPixelWidth(panel, "borderRightWidth");
			}

		// At this point newOffsetWidth is either the same as desiredOffsetWidth or is a larger value representing the 
		// minimum content size.  Search your feelings, you know it to be true.  Or just work through all the possibilities
		// in the above code.  Whatever.

		if (newOffsetWidth != desiredOffsetWidth)
			{
			// Okay, so we're larger than the desired width.  Add a few pixels for padding.
			newOffsetWidth += 3;

			// See if automatically expanding to this size would exceed the maximum.
			if (newOffsetWidth / desiredOffsetWidth > $ExpansionFactor)
				{
				newOffsetWidth = Math.floor(desiredOffsetWidth * $ExpansionFactor);
				}
			
			if (panel.offsetWidth != newOffsetWidth)
				{  
				NDCore.SetToAbsolutePosition(panel, undefined, undefined, newOffsetWidth, undefined);  
				resized = true;
				}
			}

		if (resized)
			{  this.UpdateLayout();  }
		};



	// Group: Variables
	// ________________________________________________________________________

	/* var: currentLocation
		A <NDLocation> representing the current hash path location.
	*/

	/* var: projectTitle
		The project title in HTML.
	*/

	/* var: hashChangePoller
		An object to assist with hash change polling on browsers that don't support onhashchange.  Only used in
		<AddHashChangeHandler()>.
	*/



	// Group: Layout Variables
	// ________________________________________________________________________

	/* var: sizerDragging

		If we're currently dragging a sizer, this will be an object with these members:

		sizer - The sizer DOM element.
		element - The DOM element the sizer is stretching.
		originalSizerX - The sizer's original X position.
		originalElementWidth - The element's original width.
		originalClientX - The mouse's original X position.
	*/

	/* var: desiredMenuWidth
		The width the menu panel should use, or undefined to use the default.  The actual menu width can be 
		slightly larger if needed to show the content without a horizontal scrollbar.
	*/

	/* var: desiredSummaryWidth
		The width the summary panel should use, or undefined to use the default.  The actual summary width 
		can be slightly larger if needed to show the content without a horizontal scrollbar.
	*/

	/* Constant: $ExpansionFactor
		This substitution is the maximum amount the menu or summary panel may be automatically expanded by.
		To allow a 15% expansion, set the value to 1.15.
	*/
	$ExpansionFactor = 1.333;

	};