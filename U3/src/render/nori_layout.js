const NoriLayout = (() => {
    
    let childrenLayoutFnByType = {};

    let scrollToCss = {
		none: 'visible',
		auto: 'auto',
		scroll: 'scroll',
		crop: 'hidden'
    };
    
    let doLayoutPass = () => {
        if (ctx.rootElementId === null) return;
        calculateRequiredSize(ctx.rootElement);
        let width = ctx.frameSize[0];
        let height = ctx.frameSize[1];
        spaceAllocation(ctx.rootElementId, 0, 0, width, height, 'S', 'S', null);
    };

    let getScrollParent = e => {
        let walker = e.parentNode;
        while (walker) {
            if (walker.NORI_type === 'ScrollPanel') return walker;
            walker = walker.parentNode;
        }
        return null;
    };

    let spaceAllocation = (
        elementId,
        xOffset, yOffset, // this includes the top-left margin
        usableWidth, usableHeight, // these already have margins taken out
        halign, valign, // these contain either the original element's alignment or are overridden by the panel logic.
        scrollParent) => { // the next parent up that is a scroll panel

        let e = ctx.elementById[elementId];

        let x;
        let y;
        let width;
        let height;
        
        // Raw size value applied from bridge (could be pixels or negative percent notation)
        let originalWidth = e.NORI_size[0];
        let originalHeight = e.NORI_size[1];
        
        // minimum required explicit size.
        // Note that requiredSize is always non-0 and so pxWidth/Height gets set to null only in
        // the event of a percent setting.
        let pxWidth = e.NORI_requiredSize[0];
        let pxHeight = e.NORI_requiredSize[1];

        // 0 to 1 ratio of size consumption of available space
        let ratioX = null;
        let ratioY = null;
        
        // ratio of size consumption converted to final pixels
        let perWidth = null;
        let perHeight = null;
        if (originalWidth !== null && originalWidth < 0) {
            pxWidth = null;
            ratioX = originalWidth / -100000.0;
            perWidth = Math.floor(ratioX * usableWidth);
        }
        if (originalHeight !== null && originalHeight < 0) {
            pxHeight = null;
            ratioY = originalHeight / -100000.0;
            perHeight = Math.floor(ratioY * usableHeight);
        }
        let margin = e.NORI_margin;
        
        if (e.NORI_flexibleText) {
            let sz;
            let text = e.firstChild.innerHTML;
            if (e.NORI_flexibleText == '%') {
                let percentWidth = Math.floor(usableWidth * ratioX);
                sz = calculateTextSize(text, e, percentWidth);
                sz[0] = percentWidth; // even if you don't wrap and don't reach the end, this is the logical width of the element.
            } else {
                sz = calculateTextSize(text, e, usableWidth);
                // if it doesn't reach the end, don't use the full width.
            }
            
            // regardless of how the width was determined, treat the new measurement as required width.
            pxWidth = sz[0];
            if (pxHeight !== null) {
                pxHeight = sz[1];
                
            } else {
                
            }
            ratioX = null;
            
        }
        
        // calculated width and height if there is no stretch
        let calcWidth = pxWidth === null ? perWidth : pxWidth;
        let calcHeight = pxHeight === null ? perHeight : pxHeight;
        
        switch (halign) {
            case 'S':
                width = usableWidth;
                x = xOffset;
                break;
            case 'L':
                width = calcWidth;
                x = xOffset;
                break;
            case 'R':
                width = calcWidth;
                x = xOffset + usableWidth - width;
                break;
            case 'C':
                let centerX = xOffset + calcWidth / 2;
                width = calcWidth;
                x = Math.floor(centerX - width / 2);
                break;
        }
        
        switch (valign) {
            case 'S':
                height = usableHeight;
                y = yOffset;
                break;
            case 'T':
                height = calcHeight;
                y = yOffset;
                break;
            case 'B':
                height = calcHeight;
                y = yOffset + usableHeight - height;
                break;
            case 'C':
                let centerY = yOffset + calcHeight / 2;
                height = calcHeight;
                y = Math.floor(centerY - height / 2);
                break;
        }
        
        let s = e.style;
        s.width = width + 'px';
        s.height = height + 'px';
        s.left = x + 'px';
        s.top = y + 'px';

        let borders = e.NORI_borders;
        if (borders) {
            let child = e.firstChild;
            if (borders[0] != 0 || borders[1] != 0 || borders[2] != 0 || borders[3] != 0) {
                child.style.width = width - borders[0] - borders[2] + 'px';
                child.style.height = height - borders[1] - borders[3] + 'px';
            } else {
                child.style.width = '100%';
                child.style.height = '100%';
            }
        }

        e.NORI_allocatedSize[0] = width;
        e.NORI_allocatedSize[1] = height;
        
        if (e.NORI_isPanel) {
            
            if (e.NORI_borders) {
                let b = e.NORI_borders;
                width -= b[0] + b[2];
                height -= b[1] + b[3];
            }
            
            if (e.NORI_scrollpanel) {
                s.overflowX = scrollToCss[e.NORI_scrollpanel[0]];
                s.overflowY = scrollToCss[e.NORI_scrollpanel[1]];
            }
            
            childrenLayoutFnByType[e.NORI_type](e, 0, 0, width, height);
        }
    };

    let doDockPanelChildrenLayout = (
        panel,
        xOffset,
        yOffset,
        availableWidth,
        availableHeight,
        overrideChildDock,
        useTransverseStretch) => {
            
        let childrenIds = panel.NORI_childrenIdList;
        let length = childrenIds.length - 1; // the last one just takes what's left, regardless of dock direction.
        if (length == -1) return;
        let child;
        let dock;
        let margin;
        let reqSize;
        let t;
        let pixelsConsumed;
        let usableWidth;
        let usableHeight;
        for (let i = 0; i < length; ++i) {
            child = ctx.elementById[childrenIds[i]];
            margin = child.NORI_margin;
            reqSize = child.NORI_requiredSize;
            dock = overrideChildDock == null ? child.NORI_dock : overrideChildDock;
            usableWidth = availableWidth - margin[0] - margin[2];
            usableHeight = availableHeight - margin[1] - margin[3];
            switch (dock) {
                case 'N':
                    spaceAllocation(
                        child.NORI_id, 
                        xOffset + margin[0], yOffset + margin[1],
                        usableWidth, usableHeight,
                        useTransverseStretch ? 'S' : child.NORI_align[0], 'T');
                    pixelsConsumed = margin[1] + margin[3] + child.NORI_allocatedSize[1];
                    yOffset += pixelsConsumed;
                    availableHeight -= pixelsConsumed;
                    break;
                
                case 'S':
                    spaceAllocation(
                        child.NORI_id,
                        xOffset + margin[0], yOffset + margin[1],
                        usableWidth, usableHeight,
                        useTransverseStretch ? 'S' : child.NORI_align[0], 'B');
                    pixelsConsumed = margin[1] + margin[3] + child.NORI_allocatedSize[1];
                    availableHeight -= pixelsConsumed;
                    break;
                
                case 'W':
                    spaceAllocation(
                        child.NORI_id,
                        xOffset + margin[0], yOffset + margin[1],
                        usableWidth, usableHeight,
                        'L', useTransverseStretch ? 'S' : child.NORI_align[1]);
                    pixelsConsumed = margin[0] + margin[2] + child.NORI_allocatedSize[0];
                    xOffset += pixelsConsumed;
                    availableWidth -= pixelsConsumed;
                    break;
                
                case 'E':
                    spaceAllocation(
                        child.NORI_id,
                        xOffset + margin[0], yOffset + margin[1],
                        usableWidth, usableHeight,
                        'R', useTransverseStretch ? 'S' : child.NORI_align[1]);
                    pixelsConsumed = margin[0] + margin[2] + child.NORI_allocatedSize[0];
                    availableWidth -= pixelsConsumed;
                    break;
            }
        }
        child = ctx.elementById[childrenIds[length]];
        margin = child.NORI_margin;
        t = panel.NORI_type;
        let ha = useTransverseStretch ? 'S' : t == 'FlowPanel' ? 'L' : null;
        let va = useTransverseStretch ? 'S' : t == 'StackPanel' ? 'T' : null;
        
        if (panel.NORI_scrollpanel) {
            let xscroll = panel.NORI_scrollpanel[0];
            let yscroll = panel.NORI_scrollpanel[1];
            if (xscroll == 'none') ha = 'S';
            else ha = 'L';
            if (yscroll == 'none') va = 'S';
            else va = 'T';
        }
        usableWidth = availableWidth - margin[0] - margin[2];
        usableHeight = availableHeight - margin[1] - margin[3];

        if (ha === null) {
            ha = child.NORI_align[0];
            if (ha === null) ha = 'L';
        }
        if (va === null) {
            va = child.NORI_align[1];
            if (va === null) va = 'T';
        }

        spaceAllocation(
            child.NORI_id,
            xOffset + margin[0], yOffset + margin[1],
            usableWidth, usableHeight,
            ha, va);
    };

    let docLayoutFn = function(panel, xOffset, yOffset, availableWidth, availableHeight) {
        doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight, null, true);
    };
    childrenLayoutFnByType['DockPanel'] = docLayoutFn;
    childrenLayoutFnByType['Border'] = docLayoutFn;
    childrenLayoutFnByType['ScrollPanel'] = docLayoutFn;

    childrenLayoutFnByType['StackPanel'] = function(panel, xOffset, yOffset, availableWidth, availableHeight) {
        doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight, 'N', false);
    };
    childrenLayoutFnByType['FlowPanel'] = function(panel, xOffset, yOffset, availableWidth, availableHeight) {
        doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight, 'W', false);
    };

    let calculateRequiredSize = (e) => {
        if (e.NORI_scrollIntoViewHandler) {
            let scrollParent = getScrollParent(e);
            if (scrollParent) scrollParent.NORI_scrollHandlerChildren.push(e);
        }

        if (e.NORI_isPanel) {

            if (e.NORI_type === 'ScrollPanel') {
                e.NORI_scrollHandlerChildren = [];
            }

            let elementById = ctx.elementById;
            let children = e.NORI_childrenIdList;
            let child;
            let i;
            let id;
            let length = children.length;
            let xSize = 0;
            let ySize = 0;
            let childWidth;
            let childHeight;
            let t;
            for (i = length - 1; i >= 0; --i) {
                id = children[i];
                child = elementById[id];
                calculateRequiredSize(child);
                
                childWidth = child.NORI_requiredSize[0] + child.NORI_margin[0] + child.NORI_margin[2];
                childHeight = child.NORI_requiredSize[1] + child.NORI_margin[1] + child.NORI_margin[3];
                switch (e.NORI_type) {
                    
                    case 'Border':
                        xSize = childWidth + e.NORI_borders[0] + e.NORI_borders[2];
                        ySize = childHeight + e.NORI_borders[1] + e.NORI_borders[3];
                        break;
                    
                    case 'DockPanel':
                        if (i == length - 1) {
                            xSize = childWidth;
                            ySize = childHeight;
                        } else {
                            switch (child.NORI_dock) {
                                case 'N':
                                case 'S':
                                    xSize = Math.max(xSize, childWidth);
                                    ySize += childHeight;
                                    break;
                                default:
                                    xSize += childWidth;
                                    ySize = Math.max(ySize, childHeight);
                                    break;
                            }
                        }
                        break;
                    
                    case 'ScrollPanel':
                        xSize = e.NORI_scrollpanel[0] == 'none' ? childWidth : 0;
                        ySize = e.NORI_scrollpanel[1] == 'none' ? childHeight : 0;
                        break;
                    
                    case 'StackPanel':
                        xSize = Math.max(xSize, childWidth);
                        ySize += childHeight;
                        break;
                    
                    case 'FlowPanel':
                        xSize += childWidth;
                        ySize = Math.max(ySize, childHeight);
                        break;
                        
                    default:
                        throw "calculateRequiredSize not implemented for " + e.NORI_type;
                }
            }
            e.NORI_requiredSize[0] = (e.NORI_size[0] > xSize) ? e.NORI_size[0] : xSize;
            e.NORI_requiredSize[1] = (e.NORI_size[1] > ySize) ? e.NORI_size[1] : ySize;
        } else {
            let w = e.NORI_size[0];
            let h = e.NORI_size[1];
            let sz;
            switch (e.NORI_type) {
                case 'Button':
                    if (w == null || h == null) {
                        sz = calculateTextSize(e.firstChild.innerHTML, e, null);
                        if (w == null) w = 15 + sz[0];
                        if (h == null) h = 6 + sz[1];
                    }
                    break;
                    
                case 'TextBlock':
                    t = e.firstChild.innerHTML;
                    e.NORI_flexibleText = false;
                    if (!e.NORI_wrap) {
                        sz = calculateTextSize(t, e, null);
                    } else if (w != null) {
                        if (w > 0) {
                            sz = calculateTextSize(t, e, w);
                        } else {
                            // the element has a fixed width, but this is based on a percent
                            // and can only be determined at spaceAllocation time.
                            sz = [0, h == null ? 0 : h];
                            e.NORI_flexibleText = '%';
                        }
                    } else {
                        // The element has no fixed width. The width of the element ought to be
                        // determined during the space allocation phase based on what's available,
                        // but could possibly wrap if there's not enough space.
                        sz = [0, h == null ? 0 : h];
                        e.NORI_flexibleText = 'wrap';
                    }
                    if (w == null || sz[0] > w) w = sz[0];
                    if (h == null || sz[1] > h) h = sz[1];
                    break;
                    
                default:
                    if (w == null) w = 0;
                    if (h == null) h = 0;
                    break;
            }
            
            e.NORI_requiredSize[0] = w;
            e.NORI_requiredSize[1] = h;
        }
    };

    let calculateTextSize = (html, element, width) => {
        let font = {};
        let fieldCount = 0;
        while (element && fieldCount != 4) {
            if (element.NORI_font) {
                let props = element.NORI_font;
                if (props.bold && !font.bold) { fieldCount++; font.bold = props.bold; }
                if (props.italic && !font.italic) { fieldCount++; font.italic = props.italic; }
                if (props.size && !font.size) { fieldCount++; font.size = props.size; }
                if (props.face && !font.face) { fieldCount++; font.face = props.face; }
            }
            element = element.parentNode;
        }
        let sizer = ctx.textSizer;
        let style = sizer.style;
        style.fontSize = font.size || '10pt';
        style.fontWeight = font.bold || 'normal';
        style.fontStyle = font.italic || 'normal';
        style.fontFamily = font.face || 'Arial';
        if (width === null) {
            style.whiteSpace = 'nowrap';
            style.width = 'auto';
        } else {
            style.whiteSpace = 'normal';
            style.width = width + 'px';
        }
        sizer.innerHTML = html;
        let sz = [Math.max(1, sizer.clientWidth) + 1, Math.max(1, sizer.clientHeight) + 1];
        sizer.innerHTML = '';
        return sz;
    };
  
    return {
        doLayoutPass,
    };
})();
