const NoriLayout = (() => {
    
    let childrenLayoutFnByType = {};

    let scrollToCss = {
		none: 'visible',
		auto: 'auto',
		scroll: 'scroll',
		crop: 'hidden'
    };
    
    let isFirstPass = true;
    let textWrapsFound = false;

    // add to this as FloatPanels are found in each pass.
    let roots = [];
    let currentRoot = null;

    let layoutPassNum = 0;

    let doLayoutPass = () => {
        textSizeL2Cache = textSizeL1Cache;
        textSizeL1Cache = {};

        layoutPassNum++;
        if (ctx.rootElementId === null) return;
        roots = [ctx.rootElement];
        for (let i = 0; i < roots.length; ++i) {
            currentRoot = roots[i];
            doLayoutPassImpl(currentRoot);
        }
        ctx.textSizer.innerHTML = '';
    };

    let getFloatPanelParent = e => {
        let walker = e;
        while (walker) {
            if (walker.NORI_type === 'ScrollPanel') {
                return walker;
            }
            if (walker.NORI_id == ctx.rootElementId) {
                return walker;
            }
            walker = walker.parentNode;
        }
        return null;
    };

    let parseAnchor = (s, e, sp) => {
        if (!s) return null;
        let t = s.split(':');
        let refElement;
        switch (t[1]) {
            case 'O': refElement = e; break;
            case 'H': refElement = sp; break;
            default:
                let eId = parseInt(t[1]);
                refElement = ctx.elementById[eId] || e;
                break;
        }
        let side = t[0] === '?' ? 'L' : t[0]; // L, R, or C
        let distance = parseInt(t[2]);
        return { refElement, side, distance, origin: e };
    };

    let resolveAnchor = (anchor, isXDimension) => {
        let rect = anchor.refElement.getBoundingClientRect();
        let pos;
        if (isXDimension) {
            pos = anchor.side === 'L' ? rect.left : anchor.side === 'R' ? rect.right : Math.floor((rect.left + rect.right) / 2);
        } else {
            pos = anchor.side === 'L' ? rect.top : anchor.size === 'R' ? rect.bottom : Math.floor((rect.top + rect.bottom) / 2);
        }
        pos += anchor.distance;
        let oRect = anchor.origin.getBoundingClientRect();

        return isXDimension ? (pos - oRect.left) : (pos - oRect.top);
    };

    let doFloatChildCalc = fp => {
        let anc = fp.NORI_floatAnchors;
        let scrollParent = getFloatPanelParent(fp); // either the next scroll parent up or the frame's internal Border instance

        let left = parseAnchor(anc[0], fp, scrollParent);
        let right = parseAnchor(anc[2], fp, scrollParent);
        let top = parseAnchor(anc[1], fp, scrollParent);
        let bottom = parseAnchor(anc[3], fp, scrollParent);

        let size = fp.NORI_requiredSize.slice(0);
        if (!left && !right) left = parseAnchor('?:O:0', fp, scrollParent);
        if (!top && !bottom) top = parseAnchor('?:O:0', fp, scrollParent);
        if (left && right) size[0] = null;
        if (top && bottom) size[1] = null;

        let leftX = left ? resolveAnchor(left, true) : null;
        let rightX = right ? resolveAnchor(right, true) : null;
        let topY = top ? resolveAnchor(top, false) : null;
        let bottomY = bottom ? resolveAnchor(bottom, false) : null;
        let width = size[0];
        let height = size[1];
        if (rightX !== null) {
            if (width !== null) { leftX = rightX - width; }
            else { width = rightX - leftX; }
        }
        if (bottomY !== null) {
            if (height !== null) { topY = bottomY - height; }
            else { height = bottomY - topY; }
        }
        return { width, height, left: leftX, top: topY };
    };

    let doLayoutPassImpl = (root) => {
        isFirstPass = true;
        textWrapsFound = false;
        let isFloatPanel = root.NORI_type === 'FloatPanel';
        calculateRequiredSize(root);
        let width = ctx.frameSize[0];
        let height = ctx.frameSize[1];
        let halign = 'S';
        let valign = 'S';
        let x = 0;
        let y = 0;
        
        if (isFloatPanel) {
            let t = doFloatChildCalc(root);
            width = t.width;
            height = t.height;
            halign = 'L';
            valign = 'T';
            x = t.left;
            y = t.top;
        }
        spaceAllocation(root.NORI_id, x, y, width, height, halign, valign);

        if (textWrapsFound) {
            isFirstPass = false;
            calculateRequiredSize(root);
            spaceAllocation(root.NORI_id, x, y, width, height, halign, valign);
        }
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
        halign, valign) => { // these contain either the original element's alignment or are overridden by the panel logic.

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

            if (isFirstPass) {
                e.NORI_textAvailableWrapWidth = sz[0];
                textWrapsFound = true;
            }
            
            // regardless of how the width was determined, treat the new measurement as required width.
            pxWidth = sz[0];
            if (pxHeight !== null) {
                pxHeight = sz[1];
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
        
        let s = (e.NORI_floatAnchors && (currentRoot === e)) ? e.firstChild.style : e.style;

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
        let t;
        let pixelsConsumed;
        let usableWidth;
        let usableHeight;
        for (let i = 0; i < length; ++i) {
            child = ctx.elementById[childrenIds[i]];
            margin = child.NORI_margin;
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
    childrenLayoutFnByType['FloatPanel'] = (panel, xOffset, yOffset, availableWidth, availableHeight) => {
        if (panel !== currentRoot) {
            roots.push(panel);
        } else {
            docLayoutFn(panel, xOffset, yOffset, availableWidth, availableHeight);
        }
    };

    let calculateRequiredSize = (e) => {
        e.NORI_layoutSetInPass = layoutPassNum;
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
                        
                    case 'FloatPanel':
                        if (e === currentRoot) {
                            xSize += childWidth;
                            ySize += childHeight;
                        } else {
                            xSize = 0;
                            ySize = 0;
                        }
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
                    } else {
                        if (w == null && !isFirstPass) w = e.NORI_textAvailableWrapWidth || 100; // erroneous but non-crashing fallback

                        if (w != null) {
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

    let textSizeL1Cache = {};
    let textSizeL2Cache = {};
    
    let calculateTextSize = (html, element, width) => {
        if (html === '') return [1, 1];
        let font = {};
        let fieldCount = 0;
        
        let walker = element;
        while (walker && fieldCount != 4) {
            if (walker.NORI_font) {
                let props = walker.NORI_font;
                if (props.bold && !font.bold) { fieldCount++; font.bold = props.bold; }
                if (props.italic && !font.italic) { fieldCount++; font.italic = props.italic; }
                if (props.size && !font.size) { fieldCount++; font.size = props.size; }
                if (props.face && !font.face) { fieldCount++; font.face = props.face; }
            }
            walker = walker.parentNode;
        }

        // Per element caching is redundant with L2 caching at the moment. Eventually, I
        // want to introduce limited layout updates when updates occur to segments of the
        // frame that don't affect other items, e.g. layout properties on items in scroll
        // panels shouldn't necessitate a layout pass outside of the scroll panel.
        // TODO: ^ that
        let key = [font.bold, font.italic, font.size, font.face, width === null ? '' : width, html].join('|');
        let sz;
        if (element.NORI_textSizeCache && element.NORI_textSizeCache[0] === key) {
            sz = element.NORI_textSizeCache[1];
            textSizeL1Cache[key] = sz;
        } else if (textSizeL1Cache[key]) {
            sz = textSizeL1Cache[key];
            element.NORI_textSizeCache = [key, sz];
        } else if (textSizeL2Cache[key]) {
            sz = textSizeL2Cache[key];
            textSizeL1Cache[key] = sz;
            element.NORI_textSizeCache = [key, sz];
        } else {
            let sizer = ctx.textSizer;
            let style = sizer.style;
            style.fontSize = font.size || '12pt';
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
            sz = [Math.max(1, sizer.clientWidth) + 1, Math.max(1, sizer.clientHeight) + 1];
            sizer.innerHTML = '';
            element.NORI_textSizeCache = [key, sz];
            textSizeL1Cache[key] = element.NORI_textSizeCache[1];
        }
        return sz.slice(0);
    };
  
    return {
        doLayoutPass,
    };
})();
