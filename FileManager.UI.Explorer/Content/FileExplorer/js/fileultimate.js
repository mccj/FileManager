/*!
  hey, [be]Lazy.js - v1.8.2 - 2016.10.25
  A fast, small and dependency free lazy load script (https://github.com/dinbror/blazy)
  (c) Bjoern Klinggaard - @bklinggaard - http://dinbror.dk/blazy
*/
(function (n, t) {
    typeof define == "function" && define.amd ? define(t) : typeof exports == "object" ? module.exports = t() : n.Blazy = t()
})(this, function () {
    "use strict";

    function w(n) {
        var t = n._util;
        t.elements = et(n.options);
        t.count = t.elements.length;
        t.destroyed && (t.destroyed = !1, n.options.container && (n.options.containerBindEvents ? n.options.containerBindEvents(t.validateT, !0) : i(n.options.container, function (n) {
            r(n, "scroll", t.validateT)
        })), r(window, "resize", t.saveViewportOffsetT), r(window, "resize", t.validateT), r(window, "scroll", t.validateT));
        b(n)
    }

    function b(n) {
        for (var r, t = n._util, i = 0; i < t.count; i++) r = t.elements[i], (rt(r, n.options) || a(r, n.options.successClass)) && (n.load(r), t.elements.splice(i, 1), t.count-- , i--);
        t.count === 0 && n.destroy()
    }

    function rt(t, i) {
        var f = t.getBoundingClientRect()
            , u, r;
        if (i.container && y && (u = t.closest(i.containerClass), u)) {
            if (r = u.getBoundingClientRect(), c(r, n)) {
                var e = r.top - i.offset
                    , o = r.right + i.offset
                    , s = r.bottom + i.offset
                    , h = r.left - i.offset
                    , l = {
                        top: e > n.top ? e : n.top
                        , right: o < n.right ? o : n.right
                        , bottom: s < n.bottom ? s : n.bottom
                        , left: h > n.left ? h : n.left
                    };
                return c(f, l)
            }
            return !1
        }
        return c(f, n)
    }

    function c(n, t) {
        return n.right >= t.left && n.bottom >= t.top && n.left <= t.right && n.top <= t.bottom
    }

    function k(n, o, c) {
        var w;
        if (!a(n, c.successClass) && (o || c.loadInvisible || n.offsetWidth > 0 && n.offsetHeight > 0))
            if (w = e(n, h) || e(n, c.src), w) {
                if (w === "{dynamic}") {
                    ut(n, c);
                    return
                }
                var tt = w.split(c.separator)
                    , b = tt[v && tt.length > 1 ? 1 : 0]
                    , rt = e(n, c.srcset)
                    , et = u(n, "img")
                    , k = n.parentNode
                    , ot = k && u(k, "picture");
                if (et || n.src === undefined) {
                    var y = new Image
                        , g = function () {
                            c.error && c.error(n, "invalid");
                            s(n, c.errorClass);
                            t(y, "error", g);
                            t(y, "load", nt)
                        }
                        , nt = function () {
                            et ? ot || l(n, b, rt) : n.style.backgroundImage = 'url("' + b + '")';
                            f(n, c);
                            t(y, "load", nt);
                            t(y, "error", g)
                        };
                    ot && (y = n, i(k.getElementsByTagName("source"), function (n) {
                        d(n, p, c.srcset)
                    }));
                    r(y, "error", g);
                    r(y, "load", nt);
                    l(y, b, rt, c);
                    c.loading && !ft(y) && c.loading(n)
                } else n.src = b, f(n, c)
            } else u(n, "video") ? (i(n.getElementsByTagName("source"), function (n) {
                d(n, it, c.src)
            }), n.load(), f(n, c)) : (c.error && c.error(n, "missing"), s(n, c.errorClass))
    }

    function ut(n, t) {
        var r = u(n, "img")
            , i = n.parentNode
            , e = i && u(i, "picture")
            , o = function () {
                t.error && t.error(n, "invalid");
                s(n, t.errorClass)
            }
            , h = function (i, u) {
                r ? e || l(n, i, u) : n.style.backgroundImage = 'url("' + i + '")';
                f(n, t)
            };
        t.loading && t.loading(n);
        t.dynamicLoad && t.dynamicLoad(n, h, o)
    }

    function ft(n) {
        return n.complete ? typeof n.naturalWidth != "undefined" && n.naturalWidth === 0 ? !1 : !0 : !1
    }

    function f(n, t) {
        s(n, t.successClass);
        t.success && t.success(n);
        o(n, t.src);
        o(n, t.srcset);
        i(t.breakpoints, function (t) {
            o(n, t.src)
        })
    }

    function d(n, t, i) {
        var r = e(n, i);
        r && (g(n, t, r), o(n, i))
    }

    function l(n, t, i) {
        i && g(n, p, i);
        n.src = t
    }

    function g(n, t, i) {
        n.setAttribute(t, i)
    }

    function e(n, t) {
        return n.getAttribute(t)
    }

    function o(n, t) {
        n.removeAttribute(t)
    }

    function u(n, t) {
        return n.nodeName.toLowerCase() === t
    }

    function a(n, t) {
        return (" " + n.className + " ")
            .indexOf(" " + t + " ") !== -1
    }

    function s(n, t) {
        a(n, t) || (n.className += " " + t)
    }

    function et(n) {
        for (var t = [], i = n.root.querySelectorAll(n.selector), r = i.length; r--; t.unshift(i[r]));
        return t
    }

    function nt(t) {
        n.bottom = (window.innerHeight || document.documentElement.clientHeight) + t;
        n.right = (window.innerWidth || document.documentElement.clientWidth) + t
    }

    function r(n, t, i) {
        n.attachEvent ? n.attachEvent && n.attachEvent("on" + t, i) : n.addEventListener(t, i, {
            capture: !1
            , passive: !0
        })
    }

    function t(n, t, i) {
        n.detachEvent ? n.detachEvent && n.detachEvent("on" + t, i) : n.removeEventListener(t, i, {
            capture: !1
            , passive: !0
        })
    }

    function i(n, t) {
        var r, i;
        if (n && t)
            for (r = n.length, i = 0; i < r && t(n[i], i) !== !1; i++);
    }

    function tt(n, t, i) {
        var r = 0;
        return function () {
            var u = +new Date;
            u - r < t || (r = u, n.apply(i, arguments))
        }
    }
    var h, n, v, y, it = "src"
        , p = "srcset";
    return function (r) {
        var e, u, f;
        document.querySelectorAll || (e = document.createStyleSheet(), document.querySelectorAll = function (n, t, i, r, u) {
            for (u = document.all, t = [], n = n.replace(/\[for\b/gi, "[htmlFor")
                .split(","), i = n.length; i--;) {
                for (e.addRule(n[i], "k:v"), r = u.length; r--;) u[r].currentStyle.k && t.push(u[r]);
                e.removeRule(0)
            }
            return t
        });
        u = this;
        f = u._util = {};
        f.elements = [];
        f.destroyed = !0;
        u.options = r || {};
        u.options.error = u.options.error || !1;
        u.options.offset = u.options.offset || 100;
        u.options.root = u.options.root || document;
        u.options.success = u.options.success || !1;
        u.options.selector = u.options.selector || ".b-lazy";
        u.options.separator = u.options.separator || "|";
        u.options.containerClass = u.options.container;
        u.options.container = u.options.containerClass ? document.querySelectorAll(u.options.containerClass) : !1;
        u.options.errorClass = u.options.errorClass || "b-error";
        u.options.breakpoints = u.options.breakpoints || !1;
        u.options.loadInvisible = u.options.loadInvisible || !1;
        u.options.successClass = u.options.successClass || "b-loaded";
        u.options.validateDelay = u.options.validateDelay || 25;
        u.options.saveViewportOffsetDelay = u.options.saveViewportOffsetDelay || 50;
        u.options.srcset = u.options.srcset || "data-srcset";
        u.options.src = h = u.options.src || "data-src";
        y = Element.prototype.closest;
        v = window.devicePixelRatio > 1;
        n = {};
        n.top = 0 - u.options.offset;
        n.left = 0 - u.options.offset;
        u.revalidate = function () {
            w(u)
        };
        u.load = function (n, t) {
            var r = this.options;
            n && n.length === undefined ? k(n, t, r) : i(n, function (n) {
                k(n, t, r)
            })
        };
        u.destroy = function () {
            var n = u._util;
            u.options.container && (u.options.containerBindEvents ? u.options.containerBindEvents(n.validateT, !1) : i(u.options.container, function (i) {
                t(i, "scroll", n.validateT)
            }));
            t(window, "scroll", n.validateT);
            t(window, "resize", n.validateT);
            t(window, "resize", n.saveViewportOffsetT);
            n.count = 0;
            n.elements.length = 0;
            n.destroyed = !0
        };
        f.validateT = tt(function () {
            b(u)
        }, u.options.validateDelay, u);
        f.saveViewportOffsetT = tt(function () {
            nt(u.options.offset)
        }, u.options.saveViewportOffsetDelay, u);
        nt(u.options.offset);
        i(u.options.breakpoints, function (n) {
            if (n.width >= window.screen.width) return h = n.src, !1
        });
        setTimeout(function () {
            w(u)
        })
    }
});
Ext.define("GleamTech.UI.BusyManager", {
    mixins: ["Ext.mixin.Observable"]
    , config: {
        delay: 200
    }
    , constructor: function (n) {
        this.mixins.observable.constructor.call(this, n)
    }
    , setBusy: function (n, t) {
        t = t || this;
        t.busyCount || (t.busyCount = 0);
        n ? t.busyCount++ : t.busyCount--;
        t.busyCount < 0 && (t.busyCount = 0);
        t.busyCount === 1 ? t.busyTimeout = Ext.defer(this.fireEvent, this.getDelay(), this, ["showBusy", t]) : t.busyCount === 0 && (clearTimeout(t.busyTimeout), this.fireEvent("hideBusy", t))
    }
});
Ext.define("GleamTech.UI.MessageBox", {
    extend: "Ext.window.MessageBox"
    , resizable: !0
    , destroyList: []
    , conflictText: {
        message: "There is already an item with the same name in this location."
        , actionMessage: "Please choose an action:"
        , replaceTitle: "Replace"
        , replaceDescription: "Replace the item in the destination with the new item:"
        , skipTitle: "Skip"
        , skipDescription: "No items will be changed. Leave this item in the destination:"
        , keepBothTitle: "Keep both"
        , keepBothDescription: "The new item will be renamed:"
        , nextConflicts: "Do this for the next {0} conflicts"
        , allConflicts: "Do this for all conflicts"
    }
    , initComponent: function () {
        this.callParent();
        this.msg.addCls("x-pre-wrap")
    }
    , onBoxReady: function () {
        this.callParent(arguments);
        this.toggleResizer(this.cfg && this.cfg.resizable === !0)
    }
    , reconfigure: function (n) {
        Ext.destroy(this.destroyList);
        this.destroyList.length = 0;
        this.callParent(arguments);
        this.addDetailsPanel(n);
        this.addPromptValidator(n);
        this.addCountDown(n);
        this.addItemsPanel(n);
        this.addConflictPanel(n);
        this.toggleResizer(n.resizable === !0)
    }
    , onShow: function () {
        var t, n, i, r;
        this.callParent(arguments);
        t = this;
        n = t.resizer;
        t.cfg.resizable && n && (i = t.getSize(), r = n.resizeTracker, n.minWidth = r.minWidth = i.width, n.minHeight = r.minHeight = i.height)
    }
    , onHide: function () {
        this.callParent(arguments);
        this.hidden && (Ext.destroy(this.destroyList), this.destroyList.length = 0)
    }
    , showError: function (n) {
        Ext.isString(n) && (n = {
            message: n
        });
        var t = {
            title: ""
            , icon: this.ERROR
            , buttons: this.OK
        };
        return this.show(Ext.applyIf(n, t))
    }
    , showWarning: function (n) {
        Ext.isString(n) && (n = {
            message: n
        });
        var t = {
            title: ""
            , icon: this.WARNING
            , buttons: this.OK
        };
        return this.show(Ext.applyIf(n, t))
    }
    , showInfo: function (n) {
        Ext.isString(n) && (n = {
            message: n
        });
        var t = {
            title: ""
            , icon: this.INFO
            , buttons: this.OK
        };
        return this.show(Ext.applyIf(n, t))
    }
    , showPrompt: function (n) {
        Ext.isString(n) && (n = {
            message: n
        });
        var t = {
            title: ""
            , buttons: this.OKCANCEL
            , minWidth: this.minPromptWidth
            , prompt: !0
        };
        return this.show(Ext.applyIf(n, t))
    }
    , showConfirm: function (n) {
        Ext.isString(n) && (n = {
            message: n
        });
        var t = {
            title: ""
            , icon: this.QUESTION
            , buttons: this.YESNO
        };
        return this.show(Ext.applyIf(n, t))
    }
    , showConflict: function (n) {
        Ext.isString(n) && (n = {
            message: n
        });
        n.message = Ext.String.format('<span class="x-conflict-action-title">{0}<\/span><br/>{1}', n.message || this.conflictText.message, this.conflictText.actionMessage);
        var t = {
            title: ""
            , conflictConfig: {
                remainingConflicts: undefined
            }
        };
        return this.show(Ext.applyIf(n, t))
    }
    , toggleResizer: function (n) {
        if (this.resizer) {
            n ? this.resizer.enable() : this.resizer.disable();
            for (var i = this.resizer, r = i.handles, f = r.length, e = i.possiblePositions, u, t = 0; t < f; t++)(u = i[e[r[t]]]) && u.el.setDisplayed(n)
        }
    }
    , onPromptKey: function (n, t) {
        var i = this;
        (t.keyCode === t.RETURN || t.keyCode === 10) && (i.msgButtons.ok.isVisible() && !i.msgButtons.ok.isDisabled() ? i.msgButtons.ok.handler.call(i, i.msgButtons.ok) : i.msgButtons.yes.isVisible() && i.msgButtons.yes.handler.call(i, i.msgButtons.yes))
    }
    , addDetailsPanel: function (n) {
        var r = n.detailsConfig
            , i, u, t, f, e;
        r && (i = r.data, u = r.type, i) && (Ext.applyIf(n, {
            resizable: !0
        }), t = new Ext.panel.Panel({
            title: "Details"
            , collapsible: !0
            , collapsed: !0
            , titleCollapse: !0
            , animCollapse: !1
            , layout: "fit"
            , flex: 1
            , minWidth: 400
            , height: 150
            , listeners: {
                beforecollapse: function () {
                    this.height = null
                }
                , scope: this
            }
        }), u === "html" ? t.add(new Ext.Component({
            renderTpl: ['<iframe src="javascript:&quot;&quot;" name="{frameName}" width="100%" height="100%" frameborder="0"><\/iframe>']
            , renderSelectors: {
                iframeEl: "iframe"
            }
            , listeners: {
                afterrender: function (n) {
                    try {
                        var t = n.iframeEl.dom.contentDocument || n.iframeEl.dom.contentWindow.document;
                        t.write(i);
                        t.close()
                    } catch (r) { }
                }
            }
        })) : u === "json" ? (f = function (n, t) {
            var i, r, u;
            for (i in n) r = typeof n[i] == "object", t.children || (t.children = []), u = {
                propertyName: i
                , propertyValue: r ? "" : n[i]
                , leaf: !r
                , iconCls: "x-tree-no-icon"
            }, t.children.push(u), r && f(n[i], u)
        }, e = {
            expanded: !0
        }, f(i, e), t.add(new Ext.tree.Panel({
            rootVisible: !1
            , useArrows: !0
            , columnLines: !0
            , rowLines: !0
            , cls: "x-property-tree"
            , columns: {
                items: [{
                    xtype: "treecolumn"
                    , text: "Property"
                    , dataIndex: "propertyName"
                    , autoSizeColumn: !0
                }, {
                    text: "Value"
                    , dataIndex: "propertyValue"
                    , innerCls: "x-pre"
                    , autoSizeColumn: !0
                }]
            }
            , root: e
            , viewConfig: {
                listeners: {
                    viewready: function (n) {
                        Ext.each(n.panel.columns, function (n) {
                            n.autoSizeColumn === !0 && n.autoSize()
                        })
                    }
                }
            }
            , listeners: {
                render: function (n) {
                    n.el.on("contextmenu", function (n) {
                        n.stopEvent()
                    })
                }
                , afterrender: function (n) {
                    var t = n.getView();
                    n.add(new Ext.tip.ToolTip({
                        view: t
                        , target: t.el
                        , delegate: t.cellSelector
                        , dismissDelay: 0
                        , showDelay: 800
                        , shadow: "drop"
                        , cls: "x-pre"
                        , maxWidth: null
                        , listeners: {
                            beforeshow: function (n) {
                                var t = n.view
                                    , r = t.getRecord(n.triggerElement)
                                    , i = t.getHeaderByCell(n.triggerElement)
                                        .dataIndex
                                    , u = t.getColumnManager()
                                        .getHeaderByDataIndex(i)
                                        .initialConfig.renderer
                                    , f = u ? u(r.get(i)) : r.get(i);
                                n.update(f)
                            }
                        }
                    }))
                }
            }
        }))) : t.add(new Ext.form.field.TextArea({
            readOnly: !0
            , value: i
        })), this.add(t), this.destroyList.push(t))
    }
    , addPromptValidator: function (n) {
        var r = n.promptConfig;
        if (r) {
            var u = r.validatorFn
                , f = r.selectionLength
                , t = this.textField
                , i = this.msgButtons.ok;
            if (u && (t.validator = function (n) {
                var t = u(n);
                return t === !0 ? (i.enable(), !0) : t === !1 ? (i.disable(), !0) : (i.disable(), t)
            }, t.validate(), this.userCallback = Ext.Function.createInterceptor(this.userCallback, function () {
                t.validator = null;
                i.enable()
            })), f) this.on({
                show: function () {
                    t.selectText(0, f)
                }
                , single: !0
            })
        }
    }
    , addCountDown: function (n) {
        var t = n.countDownConfig;
        if (this.countDownTask && (this.countDownTask.stop(!0), delete this.countDownTask), t) {
            var i = this
                , r = this.msgButtons[t.buttonId]
                , u = this.buttonText[t.buttonId];
            this.countDownTask = Ext.util.TaskManager.newTask({
                run: function (n) {
                    var i = t.seconds - n + 1;
                    i === 0 ? r.click() : r.setText(u + " (" + i + ")")
                }
                , interval: 1e3
                , fireOnStart: !0
                , repeat: t.seconds + 1
            });
            i.countDownTask.start();
            this.on({
                hide: function () {
                    i.countDownTask && i.countDownTask.stop(!0)
                }
                , single: !0
            })
        }
    }
    , addItemsPanel: function (n) {
        var t = n.itemsConfig;
        if (t) {
            Ext.applyIf(n, {
                resizable: !0
            });
            var i = new Ext.data.Store({
                model: t.model
            })
                , r = new GleamTech.UI.MultiView(Ext.apply({
                    store: i
                    , multipleSelection: !1
                    , flex: 1
                    , minWidth: 500
                    , height: 150
                    , width: 500
                }, t.multiViewConfig))
                , u = new GleamTech.UI.MultiViewStatusBar({
                    border: 0
                    , multiView: r
                });
            i.add(t.records);
            this.add(r);
            this.add(u);
            this.destroyList.push(r);
            this.destroyList.push(u);
            this.destroyList.push(i)
        }
    }
    , addConflictPanel: function (n) {
        var t = n.conflictConfig
            , f, i, s, r, h, c;
        if (t) {
            var u = new Ext.XTemplate(['<span class="x-conflict-action-icon"><\/span>', '<div class="x-conflict-action">', '<span class="x-conflict-action-title">{actionTitle}<\/span><br/>', '<span class="x-conflict-action-description">{actionDescription}<\/span><br/>', '<tpl if="showItemInfo">', '<div class="x-item x-layout-tiles">', '<span class="x-item-icon {iconCls}">', "<\/span>", '<div class="multiline-vertical-center">', "<div>", "<b>{itemName}<\/b>", '<tpl for="itemProperties">', '<br/><span class="x-item-value">{name}: {value}<\/span>', "<\/tpl>", "<\/div>", "<\/div>", "<\/div>", "<tpl else>", "<b>{itemName}<\/b>", "<\/tpl>", "<\/div>"])
                , e = {
                    showItemInfo: !0
                    , actionTitle: this.conflictText.replaceTitle
                    , actionDescription: this.conflictText.replaceDescription
                    , iconCls: t.getItemIconCls.fn.apply(t.getItemIconCls.scope, [t.newRecord, 48])
                    , itemName: ""
                    , itemProperties: []
                }
                , o = {
                    showItemInfo: !0
                    , actionTitle: this.conflictText.skipTitle
                    , actionDescription: this.conflictText.skipDescription
                    , iconCls: t.getItemIconCls.fn.apply(t.getItemIconCls.scope, [t.existingRecord, 48])
                    , itemName: ""
                    , itemProperties: []
                }
                , l = {
                    showItemInfo: !1
                    , actionTitle: this.conflictText.keepBothTitle
                    , actionDescription: this.conflictText.keepBothDescription
                    , itemName: t.itemCopyName
                };
            for (f = 0; f < t.columns.length; f++) i = t.columns[f], i.isPrimary ? (e.itemName = t.newRecord.data[i.dataIndex], o.itemName = t.existingRecord.data[i.dataIndex]) : (s = this.getFieldGetterWithFormatter(i.formatterFn, i.dataIndex), r = s(t.newRecord), r && e.itemProperties.push({
                name: i.text
                , value: r
            }), r = s(t.existingRecord), r && o.itemProperties.push({
                name: i.text
                , value: r
            }));
            h = new Ext.toolbar.Toolbar({
                layout: {
                    type: "vbox"
                    , align: "stretch"
                }
                , border: 0
                , defaults: {
                    margin: "0 10 8 10"
                    , textAlign: "left"
                }
                , cls: "x-conflict-toolbar"
                , items: [{
                    text: u.apply(e)
                    , handler: this.btnCallback
                    , scope: this
                    , itemId: "replace"
                }, {
                    text: u.apply(o)
                    , handler: this.btnCallback
                    , scope: this
                    , itemId: "skip"
                }, {
                    text: u.apply(l)
                    , handler: this.btnCallback
                    , scope: this
                    , itemId: "keepboth"
                }]
            });
            c = new Ext.toolbar.Toolbar({
                ui: "footer"
                , items: [{
                    xtype: "checkboxfield"
                    , hidden: t.remainingConflicts === 0
                    , boxLabel: t.remainingConflicts > 0 ? Ext.String.format(this.conflictText.nextConflicts, t.remainingConflicts) : this.conflictText.allConflicts
                    , handler: function (t, i) {
                        n.doSameChecked = i
                    }
                }, "->", this.makeButton(3)]
            });
            this.add(h);
            this.add(c);
            this.destroyList.push(h);
            this.destroyList.push(c);
            this.destroyList.push(u)
        }
    }
    , getFieldGetterWithFormatter: function (n, t) {
        return Ext.isFunction(n) ? function (i) {
            var r = i.data[t];
            return n(r, i)
        } : function (n) {
            return n.data[t]
        }
    }
});
Ext.define("GleamTech.UI.Ribbon", {
    extend: "Ext.tab.Panel"
    , floatCls: Ext.baseCSSPrefix + "border-region-slide-in"
    , initComponent: function () {
        Ext.apply(this, {
            collapsible: !0
            , hideCollapseTool: !0
            , collapseMode: "header"
            , animCollapse: !1
            , cls: "x-ribbon"
            , minHeight: 120
            , listeners: {
                afterRender: function () {
                    this.collapsed && this.makeUnfloated()
                }
            }
        });
        for (var n = 0; n < this.items.length; n++) this.initTabConfig(this.items[n]);
        this.callParent();
        this.tabBar.cls = "x-toolbar-default";
        this.tabBar.add({
            xtype: "tbfill"
        });
        this.tabBar.add(this.collapseButton = this.createCollapseButton());
        this.tabBar.un({
            click: this.tabBar.onClick
            , element: "el"
            , scope: this.tabBar
        });
        this.tabBar.on({
            mousedown: this.tabBar.onClick
            , element: "el"
            , scope: this.tabBar
        })
    }
    , updateHeader: function () {
        this.tabBar.isHeader = !0;
        this.header = this.tabBar;
        this.callParent(arguments)
    }
    , initTabConfig: function (n) {
        if (n.xtype = n.xtype || "toolbar", n.border = !1, n.layout = {
            align: "stretch"
        }, n.cls = "x-unselectable", n.tabConfig = {
            focusable: !1
        }, n.overflowHandler = "scroller", Ext.isArray(n.items))
            for (var t = 0; t < n.items.length; t++) this.initGroupConfig(n.items[t])
    }
    , initGroupConfig: function (n) {
        var i, t, r;
        if (n.xtype = n.xtype || "buttongroup", n.layout = n.layout || {
            type: "hbox"
        }, n.headerPosition = n.headerPosition || "bottom", n.frame = !1, Ext.isArray(n.items))
            for (i = 0; i < n.items.length; i++)
                if (t = n.items[i], t.xtype == "container")
                    for (t.listeners = {
                        beforeadd: function (n, t) {
                            t.isButton && (t.ui = t.ui + "-toolbar")
                        }
                    }, r = 0; r < t.items.length; r++) this.initItemConfig(t.items[r]);
                else this.initItemConfig(t)
    }
    , initItemConfig: function (n) {
        n.xtype = n.xtype || "button";
        n.focusable = !1;
        n.iconCls && (n.iconAlign == "top" || n.iconAlign == "bottom") && (n.iconCls += " " + this.getCenterCls(n.scale), n.scale == "large" && (n.text = this.getWrappedText(n.text, 8, "<br/>"), n.menu && (n.arrowVisible = !1, n.text += ' <span class="x-btn-inline-arrow"><\/span>')));
        n.menu || (n.listeners = n.listeners = n.listeners || {}, n.listeners.click = {
            fn: this.handleItemClick
            , scope: this
        })
    }
    , getWrappedText: function (n, t, i) {
        var u, f;
        if (n.length < t) return n;
        var o = n.match(/\S+/g)
            , e = []
            , r = "";
        for (u = 0; u < o.length; u++) f = o[u], r.length > 0 && (r.length > t || r.length + f.length > t) && (e.push(r), r = ""), r += r.length > 0 ? " " + f : f;
        return r.length > 0 && e.push(r), e.join(i)
    }
    , addTab: function (n) {
        this.initTabConfig(n);
        var t = new Ext.toolbar.Toolbar(n);
        this.add(t)
    }
    , insertTab: function (n, t) {
        this.initTabConfig(t);
        var i = new Ext.toolbar.Toolbar(t);
        this.insert(n, i)
    }
    , addGroup: function (n, t) {
        this.initGroupConfig(t);
        var i = new Ext.container.ButtonGroup(t);
        n.add(i)
    }
    , insertGroup: function (n, t, i) {
        this.initGroupConfig(i);
        var r = new Ext.container.ButtonGroup(i);
        n.insert(t, r)
    }
    , getCenterCls: function (n) {
        switch (n) {
            case "small":
                return "center-small-icon";
            case "medium":
                return "center-medium-icon";
            case "large":
                return "center-large-icon";
            default:
                return "center-small-icon"
        }
    }
    , createCollapseButton: function () {
        return new Ext.button.Button({
            ui: "default-toolbar"
            , iconCls: this.collapsed ? "gt-icon-down-short" : "gt-icon-up-short"
            , handler: this.onCollapseButtonClick
            , scope: this
            , focusable: !1
        })
    }
    , onCollapseButtonClick: function () {
        this.floatedFromCollapse && this.makeUnfloated();
        this.toggleCollapse()
    }
    , toggleCollapse: function () {
        this.callParent();
        this.updateTabBarUI()
    }
    , expand: function () {
        this.tabBar.activeTab.activate(!0);
        this.callParent()
    }
    , updateTabBarUI: function () {
        this.collapseButton.setIconCls(this.floatedFromCollapse ? "gt-icon-pin" : this.collapsed ? "gt-icon-down-short" : "gt-icon-up-short");
        this.collapsed ? this.tabBar.items.each(function (n) {
            if (n.isTab) n.el.on("mousedown", this.makeFloated, this)
        }, this) : this.tabBar.items.each(function (n) {
            n.isTab && n.el.un("mousedown", this.makeFloated, this)
        }, this);
        this.collapsed ? this.tabBar.activeTab.deactivate(!0) : this.tabBar.activeTab.activate(!0)
    }
    , makeFloated: function () {
        var n = this.collapsed;
        this.expand();
        this.tabBar.activeTab.activate(!0);
        this.body.hide();
        this.collapsed = n;
        this.updateLayout();
        this.addCls(this.floatCls);
        this.floatedFromCollapse = n;
        this.collapsed = !1;
        this.updateTabBarUI();
        this.updateLayout();
        this.body.show();
        Ext.getDoc()
            .on({
                mousedown: this.handleMouseDown
                , scope: this
                , capture: !0
            })
    }
    , makeUnfloated: function () {
        this.removeCls(this.floatCls);
        this.floatedFromCollapse = null;
        this.collapse();
        this.updateTabBarUI();
        Ext.getDoc()
            .un({
                mousedown: this.handleMouseDown
                , scope: this
                , capture: !0
            })
    }
    , handleMouseDown: function (n) {
        var t = n.getTarget("div[id=" + this.id + "]");
        t == null && this.makeUnfloated()
    }
    , handleItemClick: function () {
        this.floatedFromCollapse && this.makeUnfloated()
    }
    , toggleItemsUICondition: function (n) {
        Ext.isFunction(n) && (Ext.suspendLayouts(), this.items.each(function (t) {
            t.items.each(function (t) {
                var i = 0;
                t.items.each(function (t) {
                    t.xtype != "tbseparator" && (t.xtype == "container" ? t.items.each(function (t) {
                        t.toggleUICondition(n);
                        t.hidden || i++
                    }) : (t.toggleUICondition(n), t.hidden || i++))
                });
                i == 0 ? t.hide() : t.show()
            })
        }), Ext.resumeLayouts(!0))
    }
});
Ext.define("GleamTech.UI.Breadcrumb", {
    extend: "Ext.toolbar.Breadcrumb"
    , showIcons: !1
    , initComponent: function () {
        this.callParent(arguments);
        this.refreshSelectionBuffered = Ext.Function.createBuffered(this.refreshSelection, 200);
        this.fieldsToWatch = ["loaded", "expandable", this.displayField]
    }
    , updateStore: function (n, t) {
        if (this.callParent(arguments), t && t.un("update", this.onNodeChildrenChange, this), n) n.on("update", this.onNodeChildrenChange, this)
    }
    , onNodeChildrenChange: function (n, t, i, r) {
        r && this.selection && (t == this.selection || t.contains(this.selection)) && Ext.Array.each(r, function (n) {
            return Ext.Array.indexOf(this.fieldsToWatch, n) > -1 ? (this.refreshSelectionBuffered(), !1) : !0
        }, this)
    }
    , refreshSelection: function () {
        this._needsSync = !0;
        this.suspendEvent("selectionchange");
        this.updateSelection(this.selection);
        this.resumeEvent("selectionchange")
    }
    , setSelection: function (n, t, i) {
        this.isConfiguring && (i = !0);
        t && (this._needsSync = !0);
        i && this.suspendEvent("selectionchange");
        this.callParent(arguments);
        i && this.resumeEvent("selectionchange")
    }
    , updateSelection: function (n) {
        var i = this
            , e = i._buttons
            , y = []
            , h = i.items.getCount()
            , w = i._needsSync
            , b = i.getDisplayField()
            , c, l, f, a, v, r, o, t, p, s, u;
        if (Ext.suspendLayouts(), n) {
            for (r = n, s = n.get("depth"), v = s + 1, u = s; r;) {
                if (p = r.getId(), t = e[u], !w && t && t._breadcrumbNodeId === p) break;
                o = r.get(b);
                t ? t.setText(o) : t = e[u] = Ext.create({
                    xtype: i.getUseSplitButtons() ? "splitbutton" : "button"
                    , ui: i.getButtonUI()
                    , cls: i._btnCls + " " + i._btnCls + "-" + i.ui
                    , text: o
                    , showEmptyMenu: !0
                    , menu: {
                        listeners: {
                            click: "_onMenuClick"
                            , beforeshow: "_onMenuBeforeShow"
                            , scope: this
                        }
                        , alignOffset: [-34, 0]
                        , cls: "x-breadcrumb-menu"
                    }
                    , menuAlign: "tl-br?"
                    , handler: "_onButtonClick"
                    , scope: i
                });
                o.length == 0 ? (t.addCls("x-no-text"), t.setText("&#160;")) : t.removeCls("x-no-text");
                c = this.getShowIcons();
                c !== !1 && (l = r.get("glyph"), a = r.get("icon"), f = r.get("iconCls"), l ? (t.setGlyph(l), t.setIcon(null), t.setIconCls(f)) : a ? (t.setGlyph(null), t.setIconCls(null), t.setIcon(a)) : f ? (t.setGlyph(null), t.setIcon(null), t.setIconCls(f)) : c ? (t.setGlyph(null), t.setIcon(null), t.setIconCls((r.isLeaf() ? i._leafIconCls : i._folderIconCls) + "-" + i.ui)) : (t.setGlyph(null), t.setIcon(null), t.setIconCls(null)));
                t.setArrowVisible(r.hasChildNodes());
                t._breadcrumbNodeId = r.getId();
                r = r.parentNode;
                u--
            }
            if (this.showIcons === !1 && (f = n.get("iconCls"), t = e[0], f && t.setIconCls(f)), i.removeCls("x-breadcrumb-empty"), v > h) y = e.slice(h, s + 1), i.add(y);
            else
                for (u = h - 1; u >= v; u--) i.remove(i.items.items[u], !1)
        } else i.removeAll(!1), i.addCls("x-breadcrumb-empty");
        Ext.resumeLayouts(!0);
        i.fireEvent("selectionchange", i, n);
        i._needsSync = !1
    }
    , busyActive: !1
    , showBusy: function () {
        if (!this.busyActive) {
            this.busyActive = !0;
            var n = this.items.getAt(0);
            n && n.btnIconEl.addCls("x-loading")
        }
    }
    , hideBusy: function () {
        if (this.busyActive) {
            this.busyActive = !1;
            var n = this.items.getAt(0);
            n && n.btnIconEl.removeCls("x-loading")
        }
    }
    , privates: {
        _onMenuClick: function (n, t) {
            t && this.setSelection(this.getStore()
                .getNodeById(t._breadcrumbNodeId))
        }
        , _onMenuBeforeShow: function (n) {
            var t = this
                , c = t.getStore()
                    .getNodeById(n.ownerCmp._breadcrumbNodeId)
                , v = t.getDisplayField()
                , l = t.getShowMenuIcons()
                , e, i, o, s, u, h, f, a, r;
            if (c.hasChildNodes()) {
                for (e = c.childNodes, s = [], u = 0, a = e.length; u < a; u++) i = e[u], r = {
                    text: i.get(v)
                    , _breadcrumbNodeId: i.getId()
                }, l !== !1 && (o = i.get("glyph"), h = i.get("icon"), f = i.get("iconCls"), o ? (r.glyph = o, r.iconCls = f) : h ? r.icon = h : f ? r.iconCls = f : l && (r.iconCls = (i.isLeaf() ? t._leafIconCls : t._folderIconCls) + "-" + t.ui)), t.selection && (i == t.selection || i.contains(t.selection)) && (r.cls = "x-menu-item-bold"), s.push(r);
                n.removeAll();
                n.add(s)
            } else return !1
        }
    }
});
Ext.define("GleamTech.UI.SearchField", {
    extend: "Ext.form.field.Text"
    , triggers: {
        search: {
            cls: "x-form-trigger-search"
        }
        , clear: {
            cls: "x-form-trigger-clear"
            , hidden: !0
            , handler: function (n) {
                n.reset()
            }
        }
    }
    , initComponent: function () {
        this.callParent();
        this.on("change", this.updateTriggerUI, this, {
            priority: 1
        });
        this.addCls("x-no-ms-clear")
    }
    , updateTriggerUI: function (n, t) {
        t.length == 0 ? (this.triggers.clear.setHidden(!0), this.triggers.search.setHidden(!1)) : (this.triggers.search.setHidden(!0), this.triggers.clear.setHidden(!1))
    }
    , reset: function (n) {
        n && this.suspendCheckChange++;
        this.callParent();
        n && (this.suspendCheckChange-- , this.triggers.clear.setHidden(!0), this.triggers.search.setHidden(!1))
    }
});
Ext.define("GleamTech.UI.DragSelector", {
    extend: "Ext.plugin.Abstract"
    , alias: "plugin.gtdragselector"
    , mixins: ["Ext.mixin.Observable"]
    , requires: ["Ext.dd.DragTracker", "Ext.util.Region"]
    , dragging: !1
    , scrollTopStart: 0
    , scrollTop: 0
    , targetDragSelector: ".dragselect"
    , dragSafe: !1
    , scrollSpeed: 10
    , init: function (n) {
        var t = this;
        (t.view = n, n.isPanel && (t.grid = t.view.ownerGrid), t.selModel = t.view.getSelectionModel(), t.selModel.getSelectionMode() !== "SINGLE") && t.mon(t.view, "render", t.onRender, t)
    }
    , onRender: function () {
        var n = this;
        n.tracker = new Ext.dd.DragTracker({
            view: n.view
            , dragSelector: n
            , el: n.view.el
            , allowAllButtons: !0
            , preventDefault: !1
            , onBeforeStart: Ext.Function.bind(n.onBeforeStart, n)
            , onStart: Ext.Function.bind(n.onStart, n)
            , onDrag: Ext.Function.bind(n.onDrag, n)
            , onEnd: Ext.Function.bind(n.onEnd, n)
        });
        n.dragRegion = new Ext.util.Region;
        n.scroller = n.view.getEl();
        n.mon(n.scroller, "scroll", n.syncScroll, n)
    }
    , syncScroll: function (n) {
        var t = this.scroller.getScroll()
            .top;
        if (this.scrollTop = t - this.scrollTopStart, this.fillRegions(), this.dragging) this.onDrag(n, !0)
    }
    , fillAllRegions: function () {
        var n = this
            , t = n.objectsSelected = [];
        n.mainRegion = n.scroller.getRegion();
        n.bodyRegion = n.scroller.getRegion();
        n.mainRegion.right = n.bodyRegion.right -= n.scrollbarWidth;
        n.mainRegion.bottom = n.bodyRegion.bottom -= n.scrollbarHeight;
        Ext.Object.each(n.view.all.elements, function () {
            t.push(n.selModel.isSelected(t.length))
        }, n);
        n.syncScroll()
    }
    , fillRegions: function () {
        var t = this.rs = []
            , n = this.view;
        Ext.Object.each(n.all.elements, function (i, r) {
            t.push({
                region: Ext.util.Region.getRegion(r)
                , record: n.getRecord(r)
            })
        })
    }
    , onBeforeStart: function (n) {
        if (n.originalButton === 1) return !1;
        var t = n.getTarget(null, null, !0)
            , i, r;
        return t.hasCls("x-select-target") || (i = this.view.findItemByChild(t)) && (r = this.view.getRecord(i)) && this.selModel.isSelected(r) ? !1 : (this.scrollbarWidth = this.scroller.dom.offsetWidth - this.scroller.dom.clientWidth, this.scrollbarHeight = this.scroller.dom.offsetHeight - this.scroller.dom.clientHeight, n.getX() > this.scroller.getX() + this.scroller.dom.clientWidth - 2 || n.getY() > this.scroller.getY() + this.scroller.dom.clientHeight - 2) ? !1 : (this.ctrlState = n.ctrlKey, this.shiftState = n.shiftKey, Ext.menu.Manager.hideAll(), !0)
    }
    , onStart: function () {
        var n = this;
        n.scrollTopStart = n.scroller.getScroll()
            .top;
        n.fillAllRegions();
        n.createProxy()
            .show();
        n.dragging = !0;
        n.view.on("beforecontainerclick", n.cancelEvent, n, {
            single: !0
        })
    }
    , cancelEvent: function () {
        return !1
    }
    , createProxy: function () {
        if (this.proxy && !this.proxy.isDestroyed) {
            var n = this.view.getEl();
            return n.last() != this.proxy && n.appendChild(this.proxy), this.proxy
        }
        return this.proxy = this.view.getEl()
            .createChild({
                tag: "div"
                , cls: "x-view-selector"
            }), this.proxy
    }
    , onDrag: function (n, t) {
        var i = this
            , f = i.selModel
            , k = i.proxy
            , v = i.bodyRegion
            , y = i.dragRegion
            , u = i.tracker.startXY
            , r = i.tracker.getXY()
            , p = Math.min(u[0], r[0])
            , c = Math.min(u[1], r[1]) - i.scrollTop
            , w = Math.abs(u[0] - r[0])
            , l = Math.abs(u[1] - r[1]) + i.scrollTop
            , e, a;
        for (r[0] < u[0] && !t && (r[0] += 2), i.scrollTop >= 0 ? (u[1] - i.scrollTop > r[1] && (c = r[1], l = Math.abs(u[1] - r[1]) - i.scrollTop), v.top = v.y -= i.scrollTop) : (u[1] - i.scrollTop > r[1] && (c = r[1], l = Math.abs(u[1] - i.scrollTop - r[1])), v.bottom -= i.scrollTop), w == 0 && (w = 1), l == 0 && (l = 1), Ext.apply(y, {
            top: c
            , y: c
            , left: p
            , x: p
            , right: p + w
            , bottom: c + l
        }), y.constrainTo(v), k.setBox(y), e = i.scroller, a = 0; a < i.rs.length; a++) {
            var b = i.rs[a]
                , s = y.intersect(b.region)
                , o = b.record
                , h = f.isSelected(o)
                , d = i.objectsSelected[a];
            i.ctrlState ? d ? s && h ? f.deselect(o) : s || h || f.select(o, !0) : s && !h ? f.select(o, !0) : !s && h && f.deselect(o) : s && !h ? f.select(o, !0) : !s && h && f.deselect(o)
        }
        r[1] + 10 >= i.mainRegion.bottom && (Ext.isIE ? setTimeout(function () {
            e.scrollTo("top", e.getScroll()
                .top + 40)
        }, 100) : i.setScrollTop(e.getScroll()
            .top + i.scrollSpeed));
        r[1] - 10 <= i.mainRegion.top && (Ext.isIE ? setTimeout(function () {
            e.scrollTo("top", e.getScroll()
                .top - 40)
        }, 100) : i.setScrollTop(e.getScroll()
            .top - i.scrollSpeed))
    }
    , setScrollTop: function (n) {
        var t = this.scroller.dom;
        t.scrollTop = Ext.Number.constrain(n, 0, t.scrollHeight - t.clientHeight)
    }
    , onEnd: function () {
        var n = this;
        n.dragging = !1;
        n.proxy.hide();
        setTimeout(function () {
            n.view.un("beforecontainerclick", n.cancelEvent, n)
        }, 100)
    }
});
Ext.define("GleamTech.UI.StatusBar", {
    extend: "Ext.toolbar.Toolbar"
    , alternateClassName: "Ext.ux.StatusBar"
    , alias: "widget.statusbar"
    , requires: ["Ext.toolbar.TextItem"]
    , cls: "x-statusbar x-unselectable"
    , busyIconCls: "x-status-busy x-loading"
    , busyText: "&#160;"
    , autoClear: 5e3
    , emptyText: "&#160;"
    , activeThreadId: 0
    , initComponent: function () {
        var n = this.statusAlign === "right";
        this.callParent(arguments);
        this.currIconCls = this.iconCls || this.defaultIconCls;
        this.statusItem = Ext.create("Ext.toolbar.TextItem", {
            cls: "x-status-text " + (this.currIconCls || "")
            , text: this.text || this.defaultText || this.emptyText
        });
        this.secondaryStatusItem = Ext.create("Ext.toolbar.TextItem", {
            cls: "x-status-text"
            , text: this.secondaryText || this.emptyText
        });
        this.separatorItem = new Ext.toolbar.Separator({
            hidden: !0
            , style: {
                marginLeft: "4px"
                , marginRight: "4px"
            }
        });
        n ? (this.cls += " x-status-right", this.add("->"), this.add(this.statusItem)) : (this.insert(0, this.statusItem), this.insert(1, this.separatorItem), this.insert(2, this.secondaryStatusItem), this.insert(3, "->"))
    }
    , setStatus: function (n) {
        var i = this;
        if (n = n || {}, Ext.suspendLayouts(), Ext.isString(n) && (n = {
            text: n
        }), n.text !== undefined && i.setText(n.text), n.iconCls !== undefined && i.setIcon(n.iconCls), n.clear) {
            var t = n.clear
                , r = i.autoClear
                , u = {
                    useDefaults: !0
                    , anim: !0
                };
            Ext.isObject(t) ? (t = Ext.applyIf(t, u), t.wait && (r = t.wait)) : Ext.isNumber(t) ? (r = t, t = u) : Ext.isBoolean(t) && (t = u);
            t.threadId = this.activeThreadId;
            Ext.defer(i.clearStatus, r, i, [t])
        }
        return Ext.resumeLayouts(!0), i
    }
    , clearStatus: function (n) {
        var t, i, r, u;
        return (n = n || {}, t = this, i = t.statusItem, n.threadId && n.threadId !== t.activeThreadId) ? t : (r = n.useDefaults ? t.defaultText : t.emptyText, u = n.useDefaults ? t.defaultIconCls ? t.defaultIconCls : "" : "", n.anim ? i.el.puff({
            remove: !1
            , useDisplay: !0
            , callback: function () {
                i.el.show();
                t.setStatus({
                    text: r
                    , iconCls: u
                })
            }
        }) : t.setStatus({
            text: r
            , iconCls: u
        }), t)
    }
    , setText: function (n) {
        var t = this;
        return t.activeThreadId++ , t.text = n || "", t.rendered && t.statusItem.setText(t.text), t
    }
    , getText: function () {
        return this.text
    }
    , setIcon: function (n) {
        var t = this;
        return t.activeThreadId++ , n = n || "", t.rendered ? (t.currIconCls && (t.statusItem.removeCls(t.currIconCls), t.currIconCls = null), n.length > 0 && (t.statusItem.addCls(n), t.currIconCls = n)) : t.currIconCls = n, t
    }
    , onBoxReady: function () {
        this.text ? this.setPrimaryText(this.text) : this.secondaryText && this.setSecondaryText(this.secondaryText)
    }
    , busyActive: !1
    , showBusy: function () {
        this.busyActive || (this.busyActive = !0, Ext.suspendLayouts(), this.statusItem.addCls(this.busyIconCls), this.statusItem.setText(this.emptyText), this.separatorItem.hide(), this.secondaryStatusItem.setText(this.emptyText), Ext.resumeLayouts(!0))
    }
    , hideBusy: function () {
        this.busyActive && (this.busyActive = !1, Ext.suspendLayouts(), this.statusItem.removeCls(this.busyIconCls), this.statusItem.setText(this.text), this.text && this.text != this.emptyText && this.separatorItem.show(), this.secondaryStatusItem.setText(this.secondaryText), Ext.resumeLayouts(!0))
    }
    , setPrimaryText: function (n) {
        this.text = n || this.emptyText;
        this.statusItem.setText(this.text);
        this.separatorItem.hidden && this.text && this.text != this.emptyText && this.separatorItem.show();
        this.secondaryStatusItem.setText(this.secondaryText || this.emptyText)
    }
    , setSecondaryText: function (n) {
        this.secondaryText = n || this.emptyText;
        this.secondaryStatusItem.el ? this.secondaryStatusItem.el.update(this.secondaryText) : this.secondaryStatusItem.setText(this.secondaryText)
    }
});
Ext.define("GleamTech.UI.MultiView", {
    extend: "Ext.container.Container"
    , config: {
        store: null
        , columns: null
        , viewLayout: "details"
        , detailsLayoutThreshold: 1e3
        , multipleSelection: !0
        , checkboxSelection: !1
        , emptyCls: "x-grid-empty"
        , emptyText: ""
        , getItemIconCls: {
            fn: function () {
                return ""
            }
            , scope: null
        }
        , getItemThumbnailSrc: {
            fn: function () {
                return ""
            }
            , scope: null
        }
        , getItemDynamicThumbnailSrc: {
            fn: function () { }
            , scope: null
        }
        , enableDrag: !1
        , enableDrop: !1
        , enableNativeDrop: !1
        , ddGroup: null
    }
    , viewLayoutTypes: [{
        name: "extralargeicons"
        , iconSize: 256
        , component: "dataview"
        , iconPadding: 12
    }, {
        name: "largeicons"
        , iconSize: 96
        , component: "dataview"
        , iconPadding: 6
    }, {
        name: "mediumicons"
        , iconSize: 48
        , component: "dataview"
        , iconPadding: 3
    }, {
        name: "smallicons"
        , iconSize: 16
        , component: "dataview"
        , iconPadding: 0
    }, {
        name: "details"
        , iconSize: 16
        , component: "gridpanel"
        , iconPadding: 0
    }, {
        name: "tiles"
        , iconSize: 48
        , component: "dataview"
        , iconPadding: 3
    }]
    , activeViewComponent: null
    , lazyImageLoader: null
    , toolTip: null
    , deferEmptyText: !0
    , getItemNameFn: null
    , getItemTileFirstValueFn: function () {
        return ""
    }
    , getItemTileSecondValueFn: function () {
        return ""
    }
    , getItemToolTipValueFns: null
    , initComponent: function () {
        Ext.apply(this, {
            layout: {
                type: "fit"
            }
        });
        this.callParent();
        this.initColumnsConfig();
        this.createThrottledFunctions();
        this.mon(this.store, {
            scope: this
            , datachanged: this.onViewStoreDataChanged
        });
        this.setViewLayout(this.initialConfig.viewLayout, !0)
    }
    , createThrottledFunctions: function () {
        this.onSelectionChangeThrottled = GleamTech.Util.Function.throttle(this.onSelectionChange, 200, this);
        this.onViewRenderChangeThrottled = GleamTech.Util.Function.throttle(this.onViewRenderChange, 25, this, {
            leading: !1
        })
    }
    , onDestroy: function () {
        this.callParent();
        Ext.destroy(this.toolTip)
    }
    , initColumnsConfig: function () {
        for (var n, t = 0; t < this.columns.length; t++) n = this.columns[t], Ext.apply(n, {
            renderer: this.getColumnRendererWithFormatter(n.formatterFn, n.isPrimary ? this.iconColumnRenderer : this.columnRenderer)
            , scope: this
        }), n.isPrimary ? this.getItemNameFn = this.getFieldGetterWithFormatter(n.formatterFn, n.dataIndex) : (n.tdCls = "x-item-value", n.isTileFirstValue ? this.getItemTileFirstValueFn = this.getFieldGetterWithFormatter(n.tileFormatterFn || n.formatterFn, n.dataIndex) : n.isTileSecondValue && (this.getItemTileSecondValueFn = this.getFieldGetterWithFormatter(n.tileFormatterFn || n.formatterFn, n.dataIndex)), n.isToolTipValue && (this.getItemToolTipValueFns || (this.getItemToolTipValueFns = []), this.getItemToolTipValueFns.push({
            text: n.text
            , getter: this.getFieldGetterWithFormatter(n.tileFormatterFn || n.formatterFn, n.dataIndex)
        })));
        this.getItemNameFn == null && (this.getItemNameFn = this.getFieldGetterWithFormatter(this.columns[0].formatterFn, this.columns[0].dataIndex))
    }
    , getColumnRendererWithFormatter: function (n, t) {
        return Ext.isFunction(n) ? function (i, r, u) {
            return i = n(i, u), t.apply(this, [i, r, u])
        } : t
    }
    , getFieldGetterWithFormatter: function (n, t) {
        return Ext.isFunction(n) ? function (i) {
            var r = i.data[t];
            return n(r, i)
        } : function (n) {
            return n.data[t]
        }
    }
    , setViewLayout: function (n, t, i) {
        var e, r, u, f;
        if (!this.isConfiguring && (i || !this.viewLayoutLock)) {
            if (e = n.toLowerCase(), r = Ext.Array.findBy(this.viewLayoutTypes, function (n) {
                return n.name === e
            }), !r) throw new Error(Ext.String.format('"{0}" is not a valid layout type for MultiView.', n));
            (u = this.viewLayout, i || r !== u) && (this.viewLayout = r, !i && u && u.component === r.component ? this.activeView.refresh() : (Ext.suspendLayouts(), this.activeViewComponent && (f = this.activeViewComponent.getSelectionModel()
                .getSelection(), this.remove(this.activeViewComponent, !0)), this.activeViewComponent = r.component == "gridpanel" ? this.createGridPanel() : this.createDataView(), this.activeView = this.activeViewComponent.isDataView ? this.activeViewComponent : this.activeViewComponent.getView(), f && this.activeViewComponent.getSelectionModel()
                    .select(f, !1, !0), this.add(this.activeViewComponent), Ext.resumeLayouts(!0), this.deferEmptyText = !1), t || this.fireEvent("layoutchange", this, r, u))
        }
    }
    , updateCheckboxSelection: function () {
        !this.isConfiguring && this.activeViewComponent && this.setViewLayout(this.viewLayout.name, !0, !0)
    }
    , createGridPanel: function () {
        return new Ext.grid.Panel({
            store: this.store
            , columns: this.columns
            , rowLines: !1
            , forceFit: !0
            , bufferedRenderer: !0
            , emptyCls: this.emptyCls
            , viewConfig: {
                stripeRows: !1
                , emptyText: this.emptyText
                , deferEmptyText: this.deferEmptyText
                , loadMask: !1
                , markDirty: !1
                , listeners: {
                    scope: this
                    , afterrender: this.onViewAfterRender
                    , containercontextmenu: this.onContainerContextMenu
                    , itemcontextmenu: this.onItemContextMenu
                    , itemdblclick: this.onItemDblClick
                    , refresh: this.onViewRenderChangeThrottled
                    , itemadd: this.onViewRenderChangeThrottled
                    , itemupdate: this.onViewRenderChangeThrottled
                    , itemremove: this.onViewRenderChangeThrottled
                    , destroy: this.onViewDestroy
                }
                , plugins: {
                    gtdragselector: !0
                    , gtviewdragdrop: {
                        getItemIconCls: this.getItemIconCls
                        , enableDrag: this.enableDrag
                        , enableDrop: this.enableDrop
                        , ddGroup: this.ddGroup
                    }
                    , gtviewnativedrop: {
                        enableNativeDrop: this.enableNativeDrop
                    }
                }
            }
            , selModel: {
                type: this.checkboxSelection ? "checkboxmodel" : undefined
                , mode: this.multipleSelection ? "MULTI" : "SINGLE"
                , ignoreRightMouseSelection: !0
                , deselectOnContainerClick: !0
                , listeners: {
                    selectionchange: this.onSelectionChangeThrottled
                }
            }
        })
    }
    , iconColumnRenderer: function (n, t, i) {
        var u = this.getItemIconCls.fn.apply(this.getItemIconCls.scope, [i, this.viewLayout.iconSize])
            , r = '<span class="x-grid-icon ' + u + ' x-select-target"><\/span>';
        return n == null ? r : r + '<span class="x-editable x-select-target">' + n + "<\/span>"
    }
    , columnRenderer: function (n) {
        return n == null ? null : '<span class="x-select-target">' + n + "<\/span>"
    }
    , createDataView: function () {
        var n = this;
        return new Ext.view.View({
            store: this.store
            , emptyText: this.wrapEmptyText(this.emptyText)
            , deferEmptyText: this.deferEmptyText
            , loadMask: !1
            , autoScroll: !0
            , trackOver: !0
            , cls: "x-dataview x-unselectable x-grid-body"
            , overItemCls: "x-item-over"
            , itemSelector: "li"
            , tpl: ['<ul class="x-items">', '<tpl for=".">', '<li class="x-item {layoutCls}">', '<tpl if="checkbox && layout == \'smallicons\'"><div class="x-checkbox"><\/div><\/tpl>', '<span class="x-item-icon {iconCls} x-select-target">', '<tpl if="itemThumbnailSrc">', '<span class="x-item-thumbnail">', '<img class="b-lazy" data-src="{itemThumbnailSrc}" data-fallbackIconCls="{fallbackIconCls}" src="{blankImageSrc}" />', "<\/span>", "<\/tpl>", "<\/span>", '<tpl if="checkbox && layout != \'smallicons\'"><div class="x-checkbox"><\/div><\/tpl>', "<tpl if=\"layout == 'tiles'\">", '<div class="multiline-vertical-center">', "<div>", '<span class="x-editable x-select-target">{itemName}<\/span>', '<tpl if="itemFirstValue">', '<br/><span class="x-item-value x-select-target">{itemFirstValue}<\/span>', "<\/tpl>", '<tpl if="itemSecondValue">', '<br/><span class="x-item-value x-select-target">{itemSecondValue}<\/span>', "<\/tpl>", "<\/div>", "<\/div>", "<tpl elseif=\"layout == 'mediumicons' || layout == 'largeicons'\">", '<span class="line-clamp line-clamp-4"><span class="x-editable x-select-target">{itemName}<\/span><\/span>', "<tpl else>", '<span class="x-editable x-select-target">{itemName}<\/span>', "<\/tpl>", "<\/li>", "<\/tpl>", "<\/ul>"]
            , prepareData: function (t, i, r) {
                var u = {
                    blankImageSrc: Ext.BLANK_IMAGE_URL
                    , layout: n.viewLayout.name
                    , layoutCls: "x-layout-" + n.viewLayout.name
                    , itemName: n.getItemNameFn(r)
                }
                    , e, f;
                return n.viewLayout.iconSize >= 32 && (e = n.viewLayout.iconSize - n.viewLayout.iconPadding, u.itemThumbnailSrc = n.getItemThumbnailSrc.fn.apply(n.getItemThumbnailSrc.scope, [r, e])), f = n.getItemIconCls.fn.apply(n.getItemIconCls.scope, [r, n.viewLayout.iconSize]), u.itemThumbnailSrc ? u.fallbackIconCls = f : u.iconCls = f, n.viewLayout.name == "tiles" && (u.itemFirstValue = n.getItemTileFirstValueFn(r), u.itemSecondValue = n.getItemTileSecondValueFn(r)), u.checkbox = n.checkboxSelection, u
            }
            , listeners: {
                scope: this
                , afterrender: this.onViewAfterRender
                , containercontextmenu: this.onContainerContextMenu
                , itemcontextmenu: this.onItemContextMenu
                , itemdblclick: this.onItemDblClick
                , refresh: this.onViewRenderChangeThrottled
                , itemadd: this.onViewRenderChangeThrottled
                , itemupdate: this.onViewRenderChangeThrottled
                , itemremove: this.onViewRenderChangeThrottled
                , destroy: this.onViewDestroy
            }
            , selModel: {
                type: this.checkboxSelection ? "dataviewcheckboxmodel" : undefined
                , mode: this.multipleSelection ? "MULTI" : "SINGLE"
                , ignoreRightMouseSelection: !0
                , deselectOnContainerClick: !0
                , listeners: {
                    selectionchange: this.onSelectionChangeThrottled
                }
            }
            , plugins: {
                gtdragselector: !0
                , gtviewdragdrop: {
                    getItemIconCls: this.getItemIconCls
                    , enableDrag: this.enableDrag
                    , enableDrop: this.enableDrop
                    , ddGroup: this.ddGroup
                }
                , gtviewnativedrop: {
                    enableNativeDrop: this.enableNativeDrop
                }
            }
        })
    }
    , onViewStoreDataChanged: function (n) {
        var t = n.getCount();
        t !== this.oldCount && (t > this.detailsLayoutThreshold ? this.lockViewLayout("details") : this.unlockViewLayout(!0), this.oldCount = t)
    }
    , onViewAfterRender: function (n) {
        this.toolTip ? (this.toolTip.view = n, this.toolTip.setTarget(n.el), this.toolTip.delegate = n.itemSelector) : this.toolTip = new Ext.tip.ToolTip({
            view: n
            , target: n.el
            , delegate: n.itemSelector
            , dismissDelay: 0
            , showDelay: 800
            , shadow: "drop"
            , listeners: {
                render: function (n) {
                    n.el.on("contextmenu", function (n) {
                        n.stopEvent()
                    })
                }
                , beforeshow: {
                    fn: function (n) {
                        var t = n.view.getRecord(n.triggerElement)
                            , i;
                        if (!t) return !1;
                        i = this.getItemNameFn(t);
                        Ext.Array.each(this.getItemToolTipValueFns, function (n) {
                            var r = n.getter(t);
                            r && (i += "<br/>" + n.text + ": " + r)
                        });
                        n.update(i)
                    }
                    , scope: this
                }
            }
        });
        this.relayEvents(n, ["dragstart", "dropenter", "dropout", "dropover", "drop", "containerdropover", "containerdropout", "containerdrop", "dropkeychange", "nativedragenter", "nativedragmove", "nativedragleave", "nativedrop"])
    }
    , onViewDestroy: function () {
        this.destroyLazyImageLoader()
    }
    , onContainerContextMenu: function (n) {
        return n.getSelectionModel()
            .deselectAll(), this.fireEventArgs("containercontextmenu", arguments)
    }
    , onItemContextMenu: function (n, t, i, r, u) {
        var f = n.getSelectionModel()
            , e = u.getTarget(null, null, !0);
        return !e.hasCls("x-select-target") && !f.isSelected(t) ? n.fireEvent("containercontextmenu", n, u) : (f.isSelected(t) || f.selectWithEvent(t, u), this.fireEventArgs("itemcontextmenu", arguments))
    }
    , onItemDblClick: function () {
        return this.fireEventArgs("itemdblclick", arguments)
    }
    , onSelectionChange: function () {
        return this.fireEventArgs("selectionchange", arguments)
    }
    , onViewRenderChange: function () {
        return this.viewLayout.component == "dataview" && this.initLazyImageLoader(this.activeView), this.fireEvent("renderchange", this.activeView)
    }
    , initLazyImageLoader: function (n) {
        if (this.viewLayout.iconSize < 32) {
            this.destroyLazyImageLoader();
            return
        }
        if (this.lazyImageLoader) this.lazyImageLoader.revalidate();
        else {
            var t = this;
            this.lazyImageLoader = new Blazy({
                container: "#" + n.getEl()
                    .id
                , selector: "#" + n.getEl()
                    .id + " .b-lazy"
                , loadInvisible: !0
                , loading: function (n) {
                    var i = Ext.fly(n)
                        , t;
                    i.addCls("b-loading");
                    t = i.up(".x-item-icon");
                    t && t.addCls("x-loading")
                }
                , success: function (n) {
                    var t = Ext.fly(n)
                        , i;
                    t.removeCls("b-loading");
                    i = t.up(".x-item-icon");
                    i && i.removeCls("x-loading x-select-target");
                    t.addCls("x-select-target");
                    n.removeAttribute("data-fallbackIconCls")
                }
                , error: function (n) {
                    var i = Ext.fly(n)
                        , t;
                    i.removeCls("b-loading");
                    i.setDisplayed("none");
                    t = i.up(".x-item-icon");
                    t && (t.setDisplayed(!1), t.removeCls("x-loading"), t.addCls(i.getAttribute("data-fallbackIconCls")), t.setDisplayed(!0));
                    n.removeAttribute("data-fallbackIconCls")
                }
                , dynamicLoad: function (n, i, r) {
                    var u = t.activeView.getNode(n)
                        , f = t.activeView.getRecord(u)
                        , e;
                    if (!u || !f) {
                        r();
                        return
                    }
                    e = t.viewLayout.iconSize - t.viewLayout.iconPadding;
                    t.getItemDynamicThumbnailSrc.fn.apply(t.getItemDynamicThumbnailSrc.scope, [f, e, i, r])
                }
                , containerBindEvents: function (i, r) {
                    if (r) {
                        n.on("scroll", i);
                        t.on("resize", i)
                    } else n.un("scroll", i), t.un("resize", i)
                }
            })
        }
    }
    , destroyLazyImageLoader: function () {
        this.lazyImageLoader && (this.lazyImageLoader.destroy(), this.lazyImageLoader = null)
    }
    , getSelectionModel: function () {
        return this.activeViewComponent.getSelectionModel()
    }
    , clearEmptyEl: function () {
        this.activeView && this.activeView.clearEmptyEl()
    }
    , wrapEmptyText: function (n) {
        return '<div class="' + this.emptyCls + '">' + n + "<\/div>"
    }
    , setEmptyText: function (n) {
        this.emptyText = n;
        this.activeViewComponent && this.activeViewComponent.setEmptyText(this.activeViewComponent.isDataView ? this.wrapEmptyText(n) : n)
    }
    , lockViewLayout: function (n) {
        if (!this.viewLayoutLock) {
            n = n.toLowerCase();
            this.viewLayoutBeforeLock = this.viewLayout.name;
            this.setViewLayout(n);
            this.viewLayoutLock = n;
            var t = [];
            Ext.Array.each(this.viewLayoutTypes, function (i) {
                i.name !== n && t.push(i.name)
            });
            this.toggleViewLayoutState(t, !1)
        }
    }
    , unlockViewLayout: function (n) {
        var i, t;
        this.viewLayoutLock && (i = this.viewLayoutLock, delete this.viewLayoutLock, n && this.setViewLayout(this.viewLayoutBeforeLock), delete this.viewLayoutBeforeLock, t = [], Ext.Array.each(this.viewLayoutTypes, function (n) {
            n.name !== i && t.push(n.name)
        }), this.toggleViewLayoutState(t, !0))
    }
    , toggleViewLayoutState: function (n, t) {
        var i = Ext.Array.map(n, function (n) {
            return {
                name: n.toLowerCase()
                , enabled: t
            }
        });
        this.fireEvent("layoutstatechange", this.activeView, i)
    }
});
Ext.define("GleamTech.UI.DataViewCheckboxModel", {
    extend: "Ext.selection.DataViewModel"
    , alias: "selection.dataviewcheckboxmodel"
    , privates: {
        selectWithEventMulti: function (n, t, i) {
            var r = this;
            t.shiftKey || t.ctrlKey || !t.getTarget(".x-checkbox") ? r.callParent([n, t, i]) : i ? r.doDeselect(n) : r.doSelect(n, !0)
        }
    }
});
Ext.define("GleamTech.UI.MultiViewStatusBar", {
    extend: "GleamTech.UI.StatusBar"
    , mixins: ["GleamTech.UI.ActionHandler"]
    , statics: {
        sprite: new GleamTech.UI.Sprite("explorerviewicons")
    }
    , config: {
        multiView: null
        , getDataStatusText: {
            fn: function () {
                return ""
            }
            , scope: null
        }
        , getSelectionStatusText: {
            fn: function () {
                return ""
            }
            , scope: null
        }
        , layoutButtons: "details, largeicons"
    }
    , constructor: function (n) {
        this.mixins.actionHandler.constructor.call(this, n);
        this.callParent(arguments)
    }
    , initComponent: function () {
        var t, i, n, r, u;
        for (this.initActions(), t = [], i = this.layoutButtons.split(/\s?,\s?/), n = 0; n < i.length; n++) r = i[n], u = this.applyControlAction({
            toggleGroup: this.id + "layoutGroup"
            , allowDepress: !1
            , useTextAsTooltip: !0
        }, "ToggleLayout" + r, 16), t.push(u);
        Ext.apply(this, {
            defaults: {
                focusable: !1
            }
            , items: t
        });
        this.mon(this.multiView.getStore(), {
            scope: this
            , datachanged: this.onViewStoreDataChanged
        });
        this.mon(this.multiView, {
            scope: this
            , selectionchange: this.onViewSelectionChange
            , layoutchange: this.onViewLayoutChange
            , layoutstatechange: this.onViewLayoutStateChange
        });
        this.callParent();
        this.getControlAction("ToggleLayout" + this.multiView.viewLayout.name)
            .each(function (n) {
                n.toggleUIOnly(!0)
            })
    }
    , initActions: function () {
        this.addControlActions([{
            actionName: "ToggleLayoutExtraLargeIcons"
            , languageKey: "Label.Layout.ExtraLargeIcons"
            , iconName: "ExtraLargeIcons"
        }, {
            actionName: "ToggleLayoutLargeIcons"
            , languageKey: "Label.Layout.LargeIcons"
            , iconName: "LargeIcons"
        }, {
            actionName: "ToggleLayoutMediumIcons"
            , languageKey: "Label.Layout.MediumIcons"
            , iconName: "MediumIcons"
        }, {
            actionName: "ToggleLayoutSmallIcons"
            , languageKey: "Label.Layout.SmallIcons"
            , iconName: "SmallIcons"
        }, {
            actionName: "ToggleLayoutList"
            , languageKey: "Label.Layout.List"
            , iconName: "List"
        }, {
            actionName: "ToggleLayoutDetails"
            , languageKey: "Label.Layout.Details"
            , iconName: "Details"
        }, {
            actionName: "ToggleLayoutTiles"
            , languageKey: "Label.Layout.Tiles"
            , iconName: "Tiles"
        }, {
            actionName: "ToggleLayoutContent"
            , languageKey: "Label.Layout.Content"
            , iconName: "Content"
        }], this.statics()
            .sprite)
    }
    , onViewStoreDataChanged: function (n) {
        var t = n.getCount()
            , i = t === 1 ? GleamTech.Util.Language.getEntry("Label.ItemCount", t) : GleamTech.Util.Language.getEntry("Label.ItemsCount", t)
            , r = this.getDataStatusText.fn.apply(this.getDataStatusText.scope, [n.getData()
                .items]);
        r.length > 0 && (i += "&#160;&#160;" + r);
        this.setPrimaryText(i)
    }
    , onViewSelectionChange: function (n, t) {
        var i = t.length
            , r, u;
        i === 0 ? this.setSecondaryText(null) : (r = i === 1 ? GleamTech.Util.Language.getEntry("Label.SelectedItemCount", i) : GleamTech.Util.Language.getEntry("Label.SelectedItemsCount", i), u = this.getSelectionStatusText.fn.apply(this.getSelectionStatusText.scope, [t]), u.length > 0 && (r += "&#160;&#160;" + u), this.setSecondaryText(r))
    }
    , onViewLayoutChange: function (n, t, i) {
        this.getControlAction("ToggleLayout" + t.name)
            .each(function (n) {
                n.toggleUIOnly(!0)
            });
        i && this.getControlAction("ToggleLayout" + i.name)
            .each(function (n) {
                n.toggleUIOnly(!1)
            })
    }
    , onViewLayoutStateChange: function (n, t) {
        for (var r, u, i = 0; i < t.length; i++) r = t[i], u = this.getControlAction("ToggleLayout" + r.name), r.enabled ? u.enable() : u.disable()
    }
    , onActionToggleLayoutExtraLargeIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("extralargeicons")
    }
    , onActionToggleLayoutLargeIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("largeicons")
    }
    , onActionToggleLayoutMediumIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("mediumicons")
    }
    , onActionToggleLayoutSmallIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("smallicons")
    }
    , onActionToggleLayoutList: function (n) {
        n.pressed && this.multiView.setViewLayout("list")
    }
    , onActionToggleLayoutDetails: function (n) {
        n.pressed && this.multiView.setViewLayout("details")
    }
    , onActionToggleLayoutTiles: function (n) {
        n.pressed && this.multiView.setViewLayout("tiles")
    }
    , onActionToggleLayoutContent: function (n) {
        n.pressed && this.multiView.setViewLayout("content")
    }
});
Ext.define("GleamTech.UI.ReferrerIframe", {
    el: null
    , window: null
    , constructor: function (n) {
        this.parentEl = n
    }
    , setReferrer: function (n, t) {
        var r, i;
        if (this.el) {
            if (this.el.originalSrc == n) {
                Ext.callback(t, null, [!1]);
                return
            }
            r = this.el.id;
            this.el.remove();
            this.el = null
        }
        i = r || Ext.id(null, "referrer-iframe-");
        this.el = Ext.get(Ext.DomHelper.createDom({
            tag: "iframe"
            , id: i
            , name: i
            , style: "display: none"
            , src: n
        }));
        this.el.originalSrc = n;
        this.el.on("load", function () {
            Ext.callback(t, null, [!0])
        }, null, {
            single: !0
        });
        this.parentEl.appendChild(this.el);
        this.window = this.el.dom.contentWindow || window.frames[this.el.dom.name];
        !this.window.eval && this.window.execScript && this.window.execScript("null")
    }
});
Ext.define("GleamTech.UI.DragDropProxy", {
    extend: "Ext.dd.StatusProxy"
    , childEls: ["ghost", "status"]
    , renderTpl: ['<div id="{id}-ghost" data-ref="ghost" class="x-dd-item-ghost" role="presentation"><\/div><div id="{id}-status" data-ref="status" class="x-dd-status-hidden"><\/div>']
    , constructor: function (n) {
        var t = this;
        n = n || {};
        Ext.apply(t, {
            hideMode: "visibility"
            , hidden: !0
            , floating: !0
            , id: t.id || Ext.id()
            , cls: "x-dd-proxy " + Ext.baseCSSPrefix + "dd-drag-proxy " + this.dropNotAllowed
            , shadow: n.shadow || !1
            , renderTo: Ext.getDetachedBody()
        });
        t.callParent(arguments);
        this.dropStatus = this.dropNotAllowed
    }
    , reset: function (n) {
        var t = this
            , i = "x-dd-proxy " + Ext.baseCSSPrefix + "dd-drag-proxy ";
        t.el.replaceCls(i + t.dropAllowed, i + t.dropNotAllowed);
        t.dropStatus = t.dropNotAllowed;
        n && t.ghost.setHtml("")
    }
    , setStatusText: function (n, t) {
        this.status.dom.className = "x-dd-status-text " + t;
        this.status.dom.innerHTML = n;
        this.allowed = !0
    }
    , setStatusNotAllowed: function () {
        this.status.dom.className = "x-dd-status-notallowed";
        this.status.dom.innerHTML = "";
        this.allowed = !1
    }
    , setStatusHidden: function () {
        this.status.dom.className = "x-dd-status-hidden";
        this.status.dom.innerHTML = "";
        this.statusMode = "hidden";
        this.allowed = undefined
    }
});
Ext.define("GleamTech.UI.ViewDragZone", {
    extend: "Ext.view.DragZone"
    , selectOnDrag: !0
    , constructor: function (n) {
        var t, i = n.view
            , r = i.ownerCt;
        t = r ? r.getTargetEl()
            .dom : i.el.dom.parentNode;
        this.proxy = new GleamTech.UI.DragDropProxy({
            id: t.id + "-drag-proxy"
            , animRepair: this.animRepair
        });
        this.callParent(arguments);
        this.ddel.className = "x-dd-item " + this.ddel.className
    }
    , onInitDrag: function (n, t) {
        var i = this
            , r = i.dragData
            , e = r.view
            , u = e.getSelectionModel()
            , f = e.getRecord(r.item)
            , o;
        this.selectOnDrag && !u.isSelected(f) && (u.selectWithEvent(f, i.DDMInstance.mousedownEvent), r.records = u.getSelection());
        r.records.length > 1 ? Ext.fly(i.ddel)
            .setHtml('<div class="x-dd-item-number"><div>' + r.records.length + "<\/div><\/div>") : (o = this.getItemIconCls.fn.apply(this.getItemIconCls.scope, [f, 96]), Ext.fly(i.ddel)
                .setHtml('<div class="x-dd-item-icon ' + o + '"><\/div>'));
        i.proxy.update(i.ddel);
        i.onStartDrag(n, t);
        return this.view.fireEvent("dragstart", this.view, this, r), !0
    }
    , afterRepair: function () {
        var n = this;
        n.dragging = !1
    }
    , onDragKeyChange: function (n) {
        if (this.cachedTarget) this.cachedTarget.onDropKeyChange(this, n, this.dragData)
    }
    , onItemMouseDown: function (n, t, i, r, u) {
        var f;
        u.pointerType === "touch" && u.type !== "longpress" || u.position && u.position.isEqual(u.view.actionPosition) || this.isPreventDrag(u, t, i, r) || (f = n.getNavigationModel(), this.selectOnDrag ? u.position ? f.setPosition(u.position) : f.setPosition(r) : f.setPosition(null, null), this.handleMouseDown(u))
    }
});
Ext.define("GleamTech.UI.TreeDragZone", {
    extend: "GleamTech.UI.ViewDragZone"
    , selectOnDrag: !1
    , isPreventDrag: function (n, t) {
        return t.get("allowDrag") === !1 || !!n.getTarget(this.view.expanderSelector)
    }
    , getDragData: function (n) {
        var t = this.view
            , i = n.getTarget(t.getItemSelector());
        if (i) return {
            copy: t.copy || t.allowCopy && n.ctrlKey
            , event: n
            , view: t
            , ddel: this.ddel
            , item: i
            , records: [t.getRecord(i)]
            , fromPosition: Ext.fly(i)
                .getXY()
        }
    }
});
Ext.define("GleamTech.UI.ViewDropZone", {
    extend: "Ext.dd.DropZone"
    , constructor: function (n) {
        var t = this;
        Ext.apply(t, n);
        t.ddGroup || (t.ddGroup = "view-dd-zone-" + t.view.id);
        t.callParent([t.view.el])
    }
    , getTargetFromEvent: function (n) {
        return n.getTarget(this.view.itemSelector)
    }
    , onNodeEnter: function (n, t, i, r) {
        this.view.fireEvent("dropenter", this.view, i, n, t, r)
    }
    , onNodeOut: function (n, t, i, r) {
        this.view.fireEvent("dropout", this.view, i, n, t, r)
    }
    , onNodeOver: function (n, t, i, r) {
        return this.view.fireEvent("dropover", this.view, i, n, t, r)
    }
    , onNodeDrop: function (n, t, i, r) {
        return this.view.fireEvent("drop", this.view, i, n, t, r)
    }
    , onContainerOver: function (n, t, i) {
        return this.view.fireEvent("containerdropover", this.view, t, n, i)
    }
    , onContainerDrop: function (n, t, i) {
        return this.view.fireEvent("containerdrop", this.view, t, n, i)
    }
    , onDropKeyChange: function (n, t, i) {
        this.view.fireEvent("dropkeychange", this.view, t, n, i)
    }
    , notifyOut: function (n, t, i) {
        this.lastOverNode || this.view.fireEvent("containerdropout", this.view, t, n, i);
        this.callParent(arguments)
    }
});
Ext.define("GleamTech.UI.TreeDropZone", {
    extend: "GleamTech.UI.ViewDropZone"
    , expandDelay: 500
    , expandNode: function (n) {
        var t = this.view;
        this.expandProcId = !1;
        n.isLeaf() || n.isExpanded() || (t.expand(n), this.expandProcId = !1)
    }
    , queueExpand: function (n) {
        this.expandProcId = Ext.Function.defer(this.expandNode, this.expandDelay, this, [n])
    }
    , cancelExpand: function () {
        this.expandProcId && (clearTimeout(this.expandProcId), this.expandProcId = !1)
    }
    , onNodeEnter: function (n, t, i, r) {
        if (this.cancelExpand(), n !== r.item) {
            var u = this.view.getRecord(n);
            u.isLeaf() || u.isExpanded() || this.queueExpand(u)
        }
        this.callParent(arguments)
    }
    , notifyOut: function () {
        this.cancelExpand();
        this.callParent(arguments)
    }
    , notifyDrop: function () {
        return this.cancelExpand(), this.callParent(arguments)
    }
});
Ext.define("GleamTech.UI.ViewDragDropPlugin", {
    extend: "Ext.grid.plugin.DragDrop"
    , alias: "plugin.gtviewdragdrop"
    , containerScroll: !0
    , onViewRender: function (n) {
        var t = this
            , i;
        t.enableDrag && (t.containerScroll && (i = n.getEl()), t.dragZone = new GleamTech.UI.ViewDragZone(Ext.apply({
            view: n
            , ddGroup: t.dragGroup || t.ddGroup
            , dragText: t.dragText
            , containerScroll: t.containerScroll
            , scrollEl: i
            , getItemIconCls: t.getItemIconCls
        }, t.dragZone)));
        t.enableDrop && (t.dropZone = new GleamTech.UI.ViewDropZone(Ext.apply({
            view: n
            , ddGroup: t.dropGroup || t.ddGroup
        }, t.dropZone)))
    }
});
Ext.define('"GleamTech.UI.TreeDragDropPlugin', {
    extend: "Ext.tree.plugin.TreeViewDragDrop"
    , alias: "plugin.gttreedragdrop"
    , containerScroll: {
        vthresh: Ext.dd.ScrollManager.vthresh
        , hthresh: -1
    }
    , onViewRender: function (n) {
        var t = this
            , i;
        t.enableDrag && (t.containerScroll && (i = n.getEl()), t.dragZone = new GleamTech.UI.TreeDragZone(Ext.apply({
            view: n
            , ddGroup: t.dragGroup || t.ddGroup
            , dragText: t.dragText
            , displayField: t.displayField
            , repairHighlightColor: t.nodeHighlightColor
            , repairHighlight: t.nodeHighlightOnRepair
            , scrollEl: i
            , getItemIconCls: t.getItemIconCls
            , containerScroll: t.containerScroll
        }, t.dragZone)));
        t.enableDrop && (t.dropZone = new GleamTech.UI.TreeDropZone(Ext.apply({
            view: n
            , ddGroup: t.dropGroup || t.ddGroup
            , allowContainerDrops: t.allowContainerDrops
            , appendOnly: t.appendOnly
            , allowParentInserts: t.allowParentInserts
            , expandDelay: t.expandDelay
            , dropHighlightColor: t.nodeHighlightColor
            , dropHighlight: t.nodeHighlightOnDrop
            , sortOnDrop: t.sortOnDrop
        }, t.dropZone)))
    }
});
Ext.override(Ext.dd.DragDropManager, {
    addListeners: function () {
        this.callParent(arguments);
        Ext.getDoc()
            .on({
                keydown: {
                    fn: this.onKeyDown
                    , scope: this
                }
                , keyup: {
                    fn: this.onKeyUp
                    , scope: this
                }
            })
    }
    , onKeyDown: function (n) {
        if (this.dragCurrent && this.dragCurrent.onDragKeyChange) {
            var t = n.getKey();
            if (t !== this.lastKey) {
                if (this.lastKey = t, t === n.ESC) {
                    this.handleMouseUp(n);
                    return
                }
                this.dragCurrent.onDragKeyChange(n)
            }
        }
    }
    , onKeyUp: function (n) {
        if (this.dragCurrent && this.dragCurrent.onDragKeyChange) {
            this.lastKey = null;
            this.dragCurrent.onDragKeyChange(n)
        }
    }
});
Ext.define("GleamTech.UI.ViewNativeDropPlugin", {
    extend: "Ext.plugin.Abstract"
    , alias: "plugin.gtviewnativedrop"
    , enableNativeDrop: !0
    , init: function (n) {
        this.view = n;
        this.view.on("render", this.onRender, this, {
            single: !0
        })
    }
    , onRender: function (n) {
        this.enableNativeDrop && (this.dragTarget = new GleamTech.UI.ViewDropTarget({
            element: n.el
            , listeners: {
                scope: this
                , dragenter: this.onDragEnter
                , dragmove: this.onDragMove
                , dragleave: this.onDragLeave
                , drop: this.onDrop
            }
            , view: n
        }))
    }
    , onDragEnter: function (n, t) {
        t.isNative && this.cmp.fireEvent("nativedragenter", this.view, t)
    }
    , onDragMove: function (n, t) {
        t.isNative && this.cmp.fireEvent("nativedragmove", this.view, t)
    }
    , onDragLeave: function (n, t) {
        t.isNative && this.cmp.fireEvent("nativedragleave", this.view, t)
    }
    , onDrop: function (n, t) {
        t.isNative && this.cmp.fireEvent("nativedrop", this.view, t)
    }
    , destroy: function () {
        this.dragTarget = Ext.destroy(this.dragTarget);
        this.callParent()
    }
});
Ext.define("GleamTech.UI.ViewDropTarget", {
    extend: "Ext.drag.Target"
    , animRepair: !0
    , constructor: function () {
        this.callParent(arguments);
        this.containerTarget = this.getElement()
            .dom;
        this.proxy = new GleamTech.UI.DragDropProxy({
            id: this.containerTarget.id + "-native-drag-proxy"
            , animRepair: this.animRepair
        });
        this.proxy.addCls(" x-dd-proxy-native");
        this.ddel = document.createElement("div");
        this.ddel.className = Ext.baseCSSPrefix + "grid-dd-wrap";
        this.ddel.className = "x-dd-item " + this.ddel.className;
        this.enteredItems = []
    }
    , privates: {
        handleNativeDragEnter: function (n) {
            var i = this.getTargetFromEvent(n)
                , r = Ext.Array.findBy(this.enteredItems, function (n) {
                    return n.item === i
                })
                , f, t, u;
            if (r || (r = {
                item: i
                , count: 0
            }, this.enteredItems.push(r)), r.count++ , !(r.count > 1)) {
                if (f = this.enteredItems.length === 1, t = Ext.drag.Manager.getNativeDragInfo(n), t.e = n, t.item = i, t.record = i ? this.view.getRecord(i) : null, t.proxy = this.proxy, t.hasFiles = this.hasFiles(n), t.target && t.target !== this && (u = t.target, u.enteredItems.length = 0, u.hideProxy(n), u.view.isTreeView && u.cancelExpand()), i ? this.view.highlightItem(i) : this.view.clearHighlight(), f && this.showProxy(n, t), this.view.isTreeView && t.record) this.onNodeEnter(t.record);
                this.callParent(arguments)
            }
        }
        , handleNativeDragLeave: function (n) {
            var i = this.getTargetFromEvent(n)
                , u = Ext.Array.findBy(this.enteredItems, function (n) {
                    return n.item === i
                })
                , f, t, r;
            if (u && (u.count-- , !(u.count > 0))) {
                if (Ext.Array.remove(this.enteredItems, u), f = this.enteredItems.length === 0, t = Ext.drag.Manager.getNativeDragInfo(n), t.e = n, t.item = i, t.record = i ? this.view.getRecord(i) : null, f && this.hideProxy(n), this.view.isTreeView && i && this.cancelExpand(), r = this, f) t.onNativeDragLeave(r, n);
                r.hasListeners.dragleave && r.fireEvent("dragleave", r, t)
            }
        }
        , handleNativeDragMove: function (n) {
            var t = Ext.drag.Manager.getNativeDragInfo(n);
            t.e = n;
            this.moveProxy(n);
            this.callParent(arguments);
            this.setDropEffect(n, this.proxy.allowed)
        }
        , handleNativeDrop: function (n) {
            var t, i;
            n.preventDefault();
            t = this.getTargetFromEvent(n);
            this.enteredItems.length = 0;
            i = Ext.drag.Manager.getNativeDragInfo(n);
            i.e = n;
            i.item = t;
            i.record = t ? this.view.getRecord(t) : null;
            this.hideProxy(n);
            this.view.isTreeView && this.cancelExpand();
            this.callParent(arguments)
        }
    }
    , getTargetFromEvent: function (n) {
        return n.getTarget(this.view.itemSelector)
    }
    , showProxy: function (n) {
        this.proxy.ensureAttachedToBody(!0);
        this.proxy.setXY(n.getXY());
        this.proxy.reset();
        this.proxy.show()
    }
    , moveProxy: function (n) {
        this.proxy.setXY(n.getXY())
    }
    , hideProxy: function () {
        this.proxy.hide();
        this.proxy.reset(!0)
    }
    , hasFiles: function (n) {
        var i = n.browserEvent.dataTransfer
            , t;
        return !i || !i.types ? !1 : (t = Ext.Array.toArray(i.types), Ext.Array.contains(t, "Files") || Ext.Array.contains(t, "public.file-url") || Ext.Array.contains(t, "application/x-moz-file"))
    }
    , getFiles: function (n) {
        var t = n.browserEvent.dataTransfer;
        return Ext.Array.toArray(t.files)
    }
    , setDropEffect: function (n, t) {
        var i = n.browserEvent.dataTransfer;
        i && (i.dropEffect = t ? "copy" : "none")
    }
    , expandDelay: 500
    , expandNode: function (n) {
        var t = this.view;
        this.expandProcId = !1;
        n.isLeaf() || n.isExpanded() || (t.expand(n), this.expandProcId = !1)
    }
    , queueExpand: function (n) {
        this.expandProcId = Ext.Function.defer(this.expandNode, this.expandDelay, this, [n])
    }
    , cancelExpand: function () {
        this.expandProcId && (clearTimeout(this.expandProcId), this.expandProcId = !1)
    }
    , onNodeEnter: function (n) {
        this.cancelExpand();
        n.isLeaf() || n.isExpanded() || this.queueExpand(n)
    }
    , destroy: function () {
        this.proxy = this.ddel = Ext.destroy(this.proxy, this.ddel);
        this.callParent()
    }
});
Ext.define("GleamTech.UI.ExplorerView", {
    extend: "Ext.panel.Panel"
    , mixins: ["GleamTech.UI.ServerHandler", "GleamTech.UI.ActionHandler", "GleamTech.UI.EventHandler"]
    , statics: {
        sprite: new GleamTech.UI.Sprite("explorerviewicons")
    }
    , config: {
        showRibbon: !0
        , collapseRibbon: !1
        , showNavigationPane: !0
        , showPreviewPane: !1
        , showDetailsPane: !1
        , viewLayout: "details"
        , viewDetailsLayoutThreshold: 1e3
        , viewMultipleSelection: !0
        , viewCheckboxSelection: !1
    }
    , width: 800
    , height: 600
    , ribbon: null
    , navigationBar: null
    , navigationPane: null
    , centerPane: null
    , previewPane: null
    , detailsPane: null
    , statusBar: null
    , messageBox: null
    , navigationStore: null
    , viewStore: null
    , navigationSelection: null
    , viewSelection: null
    , contextMenuSelection: null
    , isNavigationSelectionValid: !1
    , canUpNavigationSelection: !1
    , navigationViewContainerContextMenu: null
    , navigationViewItemContextMenu: null
    , viewContainerContextMenu: null
    , viewItemContextMenu: null
    , busyManager: null
    , constructor: function (n) {
        this.mixins.serverHandler.constructor.call(this, n);
        this.mixins.actionHandler.constructor.call(this, n);
        this.mixins.eventHandler.constructor.call(this, n);
        this.callParent(arguments)
    }
    , initComponent: function () {
        this.viewSelection = [];
        this.viewCheckboxSelection || (this.viewCheckboxSelection = Ext.os.deviceType !== "Desktop");
        this.initLabels();
        this.initActions();
        this.initStores();
        var n = [];
        this.showRibbon && n.push(this.ribbon = this.createRibbon());
        n.push(this.navigationBar = this.createNavigationBar());
        this.showNavigationPane && n.push(this.navigationPane = this.createNavigationPane());
        n.push(this.centerPane = this.createCenterPane());
        this.showPreviewPane && n.push(this.previewPane = this.createPreviewPane());
        this.showDetailsPane && n.push(this.detailsPane = this.createDetailsPane());
        n.push(this.statusBar = this.createStatusBar(this.centerPane));
        n.push(this.messageBox = this.createMessageBox());
        Ext.apply(this, {
            defaults: {
                border: !1
            }
            , layout: {
                type: "border"
            }
            , items: n
        });
        this.busyManager = new GleamTech.UI.BusyManager({
            listeners: {
                showBusy: {
                    fn: this.onShowBusy
                    , scope: this
                }
                , hideBusy: {
                    fn: this.onHideBusy
                    , scope: this
                }
            }
        });
        this.callParent();
        this.getControlAction("ToggleLayout" + this.viewLayout)
            .each(function (n) {
                n.toggleUIOnly(!0)
            })
    }
    , onDestroy: function () {
        this.callParent();
        Ext.destroy(this.navigationStore, this.viewStore, this.navigationViewContainerContextMenu, this.navigationViewItemContextMenu, this.viewContainerContextMenu, this.viewItemContextMenu)
    }
    , initLabels: function () {
        this.emptyViewText = GleamTech.Util.Language.getEntry("Message.EmptyView");
        this.emptyNavigationViewText = this.emptyViewText
    }
    , initActions: function () {
        this.addControlActions([{
            actionName: "SelectAll"
            , languageKey: "Label.SelectAll"
            , iconName: "SelectAll"
        }, {
            actionName: "SelectNone"
            , languageKey: "Label.SelectNone"
            , iconName: "SelectNone"
        }, {
            actionName: "InvertSelection"
            , languageKey: "Label.InvertSelection"
            , iconName: "InvertSelection"
        }, {
            actionName: "ToggleNavigationPane"
            , languageKey: "Label.NavigationPane"
            , iconName: "NavigationPane"
        }, {
            actionName: "TogglePreviewPane"
            , languageKey: "Label.PreviewPane"
            , iconName: "PreviewPane"
        }, {
            actionName: "ToggleDetailsPane"
            , languageKey: "Label.DetailsPane"
            , iconName: "DetailsPane"
        }, {
            actionName: "ToggleLayoutExtraLargeIcons"
            , languageKey: "Label.Layout.ExtraLargeIcons"
            , iconName: "ExtraLargeIcons"
        }, {
            actionName: "ToggleLayoutLargeIcons"
            , languageKey: "Label.Layout.LargeIcons"
            , iconName: "LargeIcons"
        }, {
            actionName: "ToggleLayoutMediumIcons"
            , languageKey: "Label.Layout.MediumIcons"
            , iconName: "MediumIcons"
        }, {
            actionName: "ToggleLayoutSmallIcons"
            , languageKey: "Label.Layout.SmallIcons"
            , iconName: "SmallIcons"
        }, {
            actionName: "ToggleLayoutList"
            , languageKey: "Label.Layout.List"
            , iconName: "List"
        }, {
            actionName: "ToggleLayoutDetails"
            , languageKey: "Label.Layout.Details"
            , iconName: "Details"
        }, {
            actionName: "ToggleLayoutTiles"
            , languageKey: "Label.Layout.Tiles"
            , iconName: "Tiles"
        }, {
            actionName: "ToggleLayoutContent"
            , languageKey: "Label.Layout.Content"
            , iconName: "Content"
        }, {
            actionName: "ToggleItemCheckBoxes"
            , languageKey: "Label.ItemCheckBoxes"
        }, {
            actionName: "Refresh"
            , languageKey: "Label.Refresh.Verb"
            , iconName: "Refresh"
        }, {
            actionName: "Expand"
            , languageKey: "Label.Expand"
        }, {
            actionName: "Collapse"
            , languageKey: "Label.Collapse"
        }, {
            actionName: "Up"
            , iconName: "Up"
        }], this.statics()
            .sprite)
    }
    , initStores: function (n, t) {
        this.navigationStore = new Ext.data.TreeStore(Ext.merge({
            root: {
                expanded: !0
                , name: ""
                , children: []
                , iconCls: this.statics()
                    .sprite.getIcon("Home")
                    .getIconCls()
            }
            , listeners: {
                load: {
                    fn: this.onNavigationStoreLoad
                    , scope: this
                }
            }
        }, n));
        this.viewStore = new Ext.data.Store(Ext.merge({
            listeners: {
                datachanged: {
                    fn: this.onViewStoreDataChanged
                    , scope: this
                }
                , load: {
                    fn: this.onViewStoreLoad
                    , scope: this
                }
            }
        }, t))
    }
    , getViewColumnsConfig: Ext.emptyFn
    , createRibbon: function () {
        return new GleamTech.UI.Ribbon({
            collapsed: this.collapseRibbon
            , region: "north"
            , items: [{
                title: GleamTech.Util.Language.getEntry("Label.Home")
                , items: [{
                    title: GleamTech.Util.Language.getEntry("Label.Select")
                    , layout: "vbox"
                    , items: [this.applyControlAction({}, "SelectAll", 16), this.applyControlAction({}, "SelectNone", 16), this.applyControlAction({}, "InvertSelection", 16)]
                }]
            }, {
                title: GleamTech.Util.Language.getEntry("Label.View.Noun")
                , items: [{
                    title: GleamTech.Util.Language.getEntry("Label.Panes")
                    , items: [this.applyControlAction({
                        enableToggle: !0
                        , pressed: this.showNavigationPane
                        , iconAlign: "top"
                    }, "ToggleNavigationPane", 32)]
                }, {
                    title: GleamTech.Util.Language.getEntry("Label.Layout")
                    , items: [{
                        xtype: "container"
                        , layout: {
                            type: "vbox"
                            , align: "stretchmax"
                        }
                        , defaults: {
                            allowDepress: !1
                            , textAlign: "left"
                        }
                        , items: [this.applyControlAction({
                            toggleGroup: this.id + "layoutGroup"
                        }, "ToggleLayoutExtraLargeIcons", 16), this.applyControlAction({
                            toggleGroup: this.id + "layoutGroup"
                        }, "ToggleLayoutMediumIcons", 16), this.applyControlAction({
                            toggleGroup: this.id + "layoutGroup"
                        }, "ToggleLayoutDetails", 16)]
                    }, {
                        xtype: "container"
                        , layout: {
                            type: "vbox"
                            , align: "stretchmax"
                        }
                        , defaults: {
                            allowDepress: !1
                            , textAlign: "left"
                        }
                        , items: [this.applyControlAction({
                            toggleGroup: this.id + "layoutGroup"
                        }, "ToggleLayoutLargeIcons", 16), this.applyControlAction({
                            toggleGroup: this.id + "layoutGroup"
                        }, "ToggleLayoutSmallIcons", 16), this.applyControlAction({
                            toggleGroup: this.id + "layoutGroup"
                        }, "ToggleLayoutTiles", 16)]
                    }]
                }, {
                    title: GleamTech.Util.Language.getEntry("Label.ShowHide")
                    , layout: "vbox"
                    , items: [this.applyControlAction({
                        xtype: "checkboxfield"
                        , checked: this.viewCheckboxSelection
                    }, "ToggleItemCheckBoxes")]
                }]
            }]
        })
    }
    , addRibbon: function () {
        this.ribbon || (this.ribbon = this.createRibbon(), this.add(this.ribbon))
    }
    , removeRibbon: function () {
        this.ribbon && (this.remove(this.ribbon, !0), this.ribbon = null)
    }
    , createNavigationBar: function () {
        return new Ext.toolbar.Toolbar({
            region: "north"
            , cls: "x-navigationbar"
            , items: [this.applyControlAction({
                useTextAsTooltip: !0
                , focusable: !1
                , enableConditions: {
                    checkFn: function () {
                        return this.canUpNavigationSelection
                    }
                    , scope: this
                }
            }, "Up", 16), new GleamTech.UI.Breadcrumb({
                itemId: "breadcrumb"
                , region: "north"
                , buttonUI: "default-toolbar"
                , flex: 1
                , defaults: {
                    focusable: !1
                }
                , store: this.navigationStore
                , displayField: "name"
                , overflowHandler: "scroller"
                , listeners: {
                    selectionchange: {
                        fn: this.onNavigationSelectionChange
                        , scope: this
                    }
                }
                , selection: this.navigationSelection
            }), this.applyControlAction({
                useTextAsTooltip: !0
                , cls: "x-refresh-button"
                , focusable: !1
                , enableConditions: {
                    checkFn: function () {
                        return this.isNavigationSelectionValid && !this.navigationSelection.isRoot()
                    }
                    , scope: this
                }
            }, "Refresh", 16), {
                xtype: "tbspacer"
                , width: 12
            }, new GleamTech.UI.SearchField({
                itemId: "searchBox"
                , width: 200
                , listeners: {
                    scope: this
                    , buffer: 500
                    , change: this.onSearchBoxChange
                }
                , enableConditions: {
                    checkFn: function () {
                        return this.isNavigationSelectionValid
                    }
                    , scope: this
                }
            })]
        })
    }
    , addNavigationBar: function () {
        this.navigationBar || (this.navigationBar = this.createNavigationBar(), this.add(this.navigationBar))
    }
    , removeNavigationBar: function () {
        this.navigationBar && (this.remove(this.navigationBar, !0), this.navigationBar = null)
    }
    , createNavigationPane: function (n) {
        return new Ext.tree.Panel(this.mergeConfig({
            region: "west"
            , split: {
                hideSplitterWhenCollapsed: !0
            }
            , collapsible: !0
            , animCollapse: !1
            , animate: !1
            , header: !1
            , lines: !1
            , useArrows: !0
            , rootVisible: !1
            , title: GleamTech.Util.Language.getEntry("Label.NavigationPane")
            , width: 200
            , store: this.navigationStore
            , displayField: "name"
            , bufferedRenderer: !0
            , viewConfig: {
                scrollable: "vertical"
                , loadMask: !1
                , emptyText: this.emptyNavigationViewText
                , deferEmptyText: !1
                , listeners: {
                    afterrender: function (n) {
                        n.toolTip = new Ext.tip.ToolTip({
                            target: n.el
                            , delegate: n.itemSelector
                            , dismissDelay: 0
                            , showDelay: 800
                            , shadow: "drop"
                            , listeners: {
                                render: function (n) {
                                    n.el.on("contextmenu", function (n) {
                                        n.stopEvent()
                                    })
                                }
                                , beforeshow: function (t) {
                                    var i = n.getRecord(t.triggerElement)
                                        , r = i.data.name;
                                    t.update(r)
                                }
                            }
                        })
                    }
                    , containercontextmenu: {
                        fn: this.onNavigationViewContainerContextMenu
                        , scope: this
                    }
                    , itemcontextmenu: {
                        fn: this.onNavigationViewItemContextMenu
                        , scope: this
                    }
                    , viewready: {
                        fn: function (n) {
                            if (this.navigationSelection) {
                                var t = n.getSelectionModel();
                                t.select(this.navigationSelection, !1, !0)
                            }
                        }
                        , scope: this
                    }
                    , destroy: function (n) {
                        Ext.destroy(n.toolTip)
                    }
                }
            }
            , selModel: {
                listeners: {
                    selectionchange: {
                        fn: this.onNavigationSelectionChange
                        , scope: this
                    }
                }
                , ignoreRightMouseSelection: !0
                , deselectOnContainerClick: !1
            }
        }, n))
    }
    , addNavigationPane: function () {
        this.navigationPane || (this.navigationPane = this.createNavigationPane(), this.add(this.navigationPane))
    }
    , removeNavigationPane: function () {
        this.navigationPane && (this.remove(this.navigationPane, !0), this.navigationPane = null)
    }
    , createCenterPane: function (n) {
        return new GleamTech.UI.MultiView(this.mergeConfig({
            region: "center"
            , store: this.viewStore
            , columns: this.getViewColumnsConfig()
            , viewLayout: this.viewLayout
            , detailsLayoutThreshold: this.viewDetailsLayoutThreshold
            , multipleSelection: this.viewMultipleSelection
            , checkboxSelection: this.viewCheckboxSelection
            , emptyText: this.emptyViewText
            , getItemIconCls: {
                fn: this.getItemIconCls
                , scope: this
            }
            , getItemThumbnailSrc: {
                fn: this.getItemThumbnailSrc
                , scope: this
            }
            , listeners: {
                scope: this
                , containercontextmenu: this.onViewContainerContextMenu
                , itemcontextmenu: this.onViewItemContextMenu
                , itemdblclick: this.onViewItemDblClick
                , selectionchange: this.onViewSelectionChange
                , layoutchange: this.onViewLayoutChange
                , layoutstatechange: this.onViewLayoutStateChange
            }
        }, n))
    }
    , addCenterPane: function () {
        this.centerPane || (this.centerPane = this.createCenterPane(), this.add(this.centerPane))
    }
    , removeCenterPane: function () {
        this.centerPane && (this.remove(this.centerPane, !0), this.centerPane = null)
    }
    , createPreviewPane: function () {
        return new Ext.panel.Panel({
            region: "east"
            , split: !0
            , collapsible: !0
            , animCollapse: !1
            , header: !0
            , title: GleamTech.Util.Language.getEntry("Label.PreviewPane")
            , width: 200
        })
    }
    , addPreviewPane: function () {
        this.previewPane || (this.previewPane = this.createPreviewPane(), this.add(this.previewPane))
    }
    , removePreviewPane: function () {
        this.previewPane && (this.remove(this.previewPane, !0), this.previewPane = null)
    }
    , createDetailsPane: function () {
        return new Ext.panel.Panel({
            region: "east"
            , split: !0
            , collapsible: !0
            , animCollapse: !1
            , header: !0
            , title: GleamTech.Util.Language.getEntry("Label.DetailsPane")
            , width: 200
        })
    }
    , addDetailsPane: function () {
        this.detailsPane || (this.detailsPane = this.createDetailsPane(), this.add(this.detailsPane))
    }
    , removeDetailsPane: function () {
        this.detailsPane && (this.remove(this.detailsPane, !0), this.detailsPane = null)
    }
    , createStatusBar: function (n) {
        return new GleamTech.UI.MultiViewStatusBar({
            region: "south"
            , multiView: n
            , getSelectionStatusText: {
                fn: this.getSelectionStatusText
                , scope: this
            }
        })
    }
    , addStatusBar: function () {
        this.statusBar || (this.statusBar = this.createStatusBar(), this.add(this.statusBar))
    }
    , removeStatusBar: function () {
        this.statusBar && (this.remove(this.statusBar, !0), this.statusBar = null)
    }
    , createMessageBox: function () {
        return new GleamTech.UI.MessageBox({
            buttonText: {
                ok: GleamTech.Util.Language.getEntry("Label.OK")
                , yes: GleamTech.Util.Language.getEntry("Label.Yes")
                , no: GleamTech.Util.Language.getEntry("Label.No")
                , cancel: GleamTech.Util.Language.getEntry("Label.Cancel")
            }
        })
    }
    , createNavigationViewContainerContextMenu: function () {
        return new Ext.menu.Menu({
            listeners: {
                beforeshow: {
                    fn: this.onNavigationViewContainerContextMenuBeforeShow
                    , scope: this
                }
            }
        })
    }
    , createNavigationViewItemContextMenu: function () {
        return new Ext.menu.Menu({
            listeners: {
                beforeshow: {
                    fn: this.onNavigationViewItemContextMenuBeforeShow
                    , scope: this
                }
            }
        })
    }
    , createViewContainerContextMenu: function () {
        return new Ext.menu.Menu({
            listeners: {
                beforeshow: {
                    fn: this.onViewContainerContextMenuBeforeShow
                    , scope: this
                }
            }
        })
    }
    , createViewItemContextMenu: function () {
        return new Ext.menu.Menu({
            listeners: {
                beforeshow: {
                    fn: this.onViewItemContextMenuBeforeShow
                    , scope: this
                }
            }
        })
    }
    , getItemIconCls: function () {
        return ""
    }
    , getItemThumbnailSrc: function () {
        return ""
    }
    , onServerHandlerMethodBegin: function (n) {
        n.busyTarget = n.operation && n.operation.node && n.operation.node.isLoading() ? n.operation.node : null;
        this.busyManager.setBusy(!0, n.busyTarget)
    }
    , onServerHandlerMethodEnd: function (n, t, i) {
        this.busyManager.setBusy(!1, n.busyTarget);
        i && (i.sessionExpired ? this.messageBox.showConfirm({
            title: GleamTech.Util.Language.getEntry("Label.RefreshPage")
            , message: GleamTech.Util.Language.getEntry("Message.SessionExpiredRefresh", 10)
            , fn: function (n) {
                n === "ok" && GleamTech.Util.Dom.refreshPage()
            }
            , buttons: Ext.Msg.OKCANCEL
            , countDownConfig: {
                buttonId: "ok"
                , seconds: 10
            }
        }) : this.messageBox.showError(i))
    }
    , checkNavigationSelection: function (n) {
        return n != null
    }
    , setNavigationSelection: function (n) {
        var u = this.navigationSelection
            , i, r, t;
        this.navigationSelection = n || null;
        this.isNavigationSelectionValid = this.checkNavigationSelection(n);
        this.canUpNavigationSelection = this.isNavigationSelectionValid && this.checkNavigationSelection(n.parentNode);
        this.navigationBar && (i = this.navigationBar.getComponent("breadcrumb"), i.getSelection() != this.navigationSelection && i.setSelection(this.navigationSelection, !0, !0), r = this.navigationBar.getComponent("searchBox"), r.setEmptyText(this.isNavigationSelectionValid ? GleamTech.Util.Language.getEntry("Label.SearchSelected", this.navigationSelection.data.name) : ""), r.reset(!0));
        this.navigationPane && (t = this.navigationPane.getSelectionModel(), this.navigationSelection ? t.getSelection()[0] != this.navigationSelection && t.select(this.navigationSelection, !1, !0) : u && t.deselect(u, !0));
        this.viewStore.clearFilter(!0);
        this.isNavigationSelectionValid || (this.viewStore.removeAll(), this.centerPane.clearEmptyEl(), this.statusBar.setPrimaryText(null))
    }
    , onNavigationSelectionChange: function (n, t) {
        this.setNavigationSelection(Ext.isArray(t) ? t[0] : t)
    }
    , onNavigationStoreLoad: function (n, t, i, r, u) {
        i || u.collapse()
    }
    , onViewStoreDataChanged: Ext.emptyFn
    , onViewStoreLoad: Ext.emptyFn
    , onViewLayoutChange: function (n, t, i) {
        this.getControlAction("ToggleLayout" + t.name)
            .each(function (n) {
                n.toggleUIOnly(!0)
            });
        i && this.getControlAction("ToggleLayout" + i.name)
            .each(function (n) {
                n.toggleUIOnly(!1)
            })
    }
    , onViewLayoutStateChange: function (n, t) {
        for (var r, u, i = 0; i < t.length; i++) r = t[i], u = this.getControlAction("ToggleLayout" + r.name), r.enabled ? u.enable() : u.disable()
    }
    , onViewSelectionChange: function (n, t) {
        this.viewSelection = t
    }
    , getSelectionStatusText: function () {
        return ""
    }
    , onSearchBoxChange: function (n, t) {
        t.length == 0 ? this.viewStore.clearFilter() : this.viewStore.getFilters()
            .replaceAll({
                property: "name"
                , anyMatch: !0
                , value: t
            })
    }
    , showContextMenu: function (n, t, i) {
        n.stopEvent();
        this.contextMenuSelection = i;
        t.showAt(n.getXY())
    }
    , onNavigationViewContainerContextMenu: function (n, t) {
        this.navigationViewContainerContextMenu || (this.navigationViewContainerContextMenu = this.createNavigationViewContainerContextMenu());
        this.showContextMenu(t, this.navigationViewContainerContextMenu)
    }
    , onNavigationViewItemContextMenu: function (n, t, i, r, u) {
        if (this.navigationViewItemContextMenu || (this.navigationViewItemContextMenu = this.createNavigationViewItemContextMenu()), this.showContextMenu(u, this.navigationViewItemContextMenu, t), !this.navigationViewItemContextMenu.hidden && !this.navigationPane.getSelectionModel()
            .isSelected(t)) {
            var f = n.indexOf(t);
            n.onRowSelect(f);
            this.navigationViewItemContextMenu.on("hide", function () {
                n.onRowDeselect(f)
            }, this, {
                single: !0
            })
        }
    }
    , onViewContainerContextMenu: function (n, t) {
        this.viewContainerContextMenu || (this.viewContainerContextMenu = this.createViewContainerContextMenu());
        this.showContextMenu(t, this.viewContainerContextMenu)
    }
    , onViewItemContextMenu: function (n, t, i, r, u) {
        this.viewItemContextMenu || (this.viewItemContextMenu = this.createViewItemContextMenu());
        this.showContextMenu(u, this.viewItemContextMenu, t)
    }
    , onViewItemDblClick: Ext.emptyFn
    , onNavigationViewContainerContextMenuBeforeShow: Ext.emptyFn
    , onNavigationViewItemContextMenuBeforeShow: Ext.emptyFn
    , onViewContainerContextMenuBeforeShow: Ext.emptyFn
    , onViewItemContextMenuBeforeShow: Ext.emptyFn
    , onShowBusy: function (n) {
        var t, r, i;
        if (n.isNode) {
            if (t = n, t.busyActive) return;
            t.busyActive = !0;
            r = Ext.get(t.getOwnerTree()
                .getView()
                .getNode(t));
            r.down(".x-tree-icon")
                .addCls("x-loading")
        } else i = this.navigationBar && this.navigationBar.getComponent("breadcrumb"), i ? i.showBusy() : this.statusBar && this.statusBar.showBusy()
    }
    , onHideBusy: function (n) {
        var t, r, i;
        if (n.isNode) {
            if (t = n, !t.busyActive) return;
            t.busyActive = !1;
            r = Ext.get(t.getOwnerTree()
                .getView()
                .getNode(t));
            r.down(".x-tree-icon")
                .removeCls("x-loading")
        } else i = this.navigationBar && this.navigationBar.getComponent("breadcrumb"), i ? i.hideBusy() : this.statusBar && this.statusBar.hideBusy()
    }
    , onControlActionHandlerBegin: function (n) {
        if (n.isMenuItem) {
            var t = n.getRootMenu();
            switch (t) {
                case this.navigationViewContextMenu:
                    return GleamTech.UI.ExplorerViewActionContext.NavigationView;
                case this.navigationViewItemContextMenu:
                    return GleamTech.UI.ExplorerViewActionContext.NavigationViewItem;
                case this.viewItemContextMenu:
                    return GleamTech.UI.ExplorerViewActionContext.ViewItem
            }
        }
        return GleamTech.UI.ExplorerViewActionContext.View
    }
    , onActionSelectAll: function () {
        this.centerPane.getSelectionModel()
            .selectAll()
    }
    , onActionSelectNone: function () {
        this.centerPane.getSelectionModel()
            .deselectAll()
    }
    , onActionInvertSelection: function () {
        var n = this.centerPane.getSelectionModel();
        n.selectAll(!0);
        n.deselect(this.viewSelection)
    }
    , onActionToggleNavigationPane: function (n) {
        n.pressed ? this.addNavigationPane() : this.removeNavigationPane()
    }
    , onActionTogglePreviewPane: function (n) {
        n.pressed ? this.addPreviewPane() : this.removePreviewPane()
    }
    , onActionToggleDetailsPane: function (n) {
        n.pressed ? this.addDetailsPane() : this.removeDetailsPane()
    }
    , onActionToggleLayoutExtraLargeIcons: function (n) {
        n.pressed && this.centerPane.setViewLayout("extralargeicons")
    }
    , onActionToggleLayoutLargeIcons: function (n) {
        n.pressed && this.centerPane.setViewLayout("largeicons")
    }
    , onActionToggleLayoutMediumIcons: function (n) {
        n.pressed && this.centerPane.setViewLayout("mediumicons")
    }
    , onActionToggleLayoutSmallIcons: function (n) {
        n.pressed && this.centerPane.setViewLayout("smallicons")
    }
    , onActionToggleLayoutList: function (n) {
        n.pressed && this.centerPane.setViewLayout("list")
    }
    , onActionToggleLayoutDetails: function (n) {
        n.pressed && this.centerPane.setViewLayout("details")
    }
    , onActionToggleLayoutTiles: function (n) {
        n.pressed && this.centerPane.setViewLayout("tiles")
    }
    , onActionToggleLayoutContent: function (n) {
        n.pressed && this.centerPane.setViewLayout("content")
    }
    , onActionToggleItemCheckBoxes: function (n) {
        this.centerPane.setCheckboxSelection(n.checked)
    }
    , onActionExpand: function () {
        this.contextMenuSelection.expand()
    }
    , onActionCollapse: function () {
        this.contextMenuSelection.collapse()
    }
    , onActionUp: function () {
        this.setNavigationSelection(this.navigationSelection.parentNode)
    }
});
GleamTech.UI.ExplorerViewLayoutType = {
    ExtraLargeIcons: 1
    , LargeIcons: 2
    , MediumIcons: 3
    , SmallIcons: 4
    , List: 5
    , Details: 6
    , Tiles: 7
    , Content: 8
};
GleamTech.UI.ExplorerViewActionContext = {
    NavigationView: 1
    , NavigationViewItem: 2
    , View: 3
    , ViewItem: 4
};
Ext.define("GleamTech.FileUltimate.FileTypeInfo", {
    singleton: !0
    , baseFileIcons: {}
    , fileIcons: {}
    , fileIconMappings: {}
    , thumbnailTypes: {}
    , archiveTypes: {}
    , imageViewerTypes: {}
    , mediaPlayerTypes: {
        mp4: {
            isVideo: !0
        }
        , m4v: {
            isVideo: !0
        }
        , f4v: {
            isVideo: !0
        }
        , mov: {
            isVideo: !0
        }
        , flv: {
            isVideo: !0
        }
        , webm: {
            isVideo: !0
        }
        , ogv: {
            isVideo: !0
        }
        , mp4v: {
            isVideo: !0
            , format: "mp4"
        }
        , "3g2": {
            isVideo: !0
            , format: "mp4"
        }
        , "3gp": {
            isVideo: !0
            , format: "mp4"
        }
        , mkv: {
            isVideo: !0
            , format: "mp4"
        }
        , aac: {
            isAudio: !0
        }
        , m4a: {
            isAudio: !0
        }
        , f4a: {
            isAudio: !0
        }
        , mp3: {
            isAudio: !0
        }
        , ogg: {
            isAudio: !0
        }
        , oga: {
            isAudio: !0
        }
        , vorbis: {
            isAudio: !0
        }
    }
    , documentViewerTypes: {}
    , normalizeExtension: function (n) {
        return n.charAt(0) === "." && (n = n.substr(1)), n.toLowerCase()
    }
    , getBaseIconCls: function (n, t) {
        n = n.toLowerCase();
        var i, r = this.baseFileIcons[n];
        return r && (i = r[t], i) ? i : ""
    }
    , getIconCls: function (n, t) {
        var i, r, u;
        return (n = this.normalizeExtension(n), r = this.fileIcons[n], r && (i = r[t], i)) ? i : (u = this.fileIconMappings[n], r = this.fileIcons[u], r && (i = r[t], i)) ? i : this.getBaseIconCls("File", t)
    }
    , getThumbnailType: function (n) {
        return this.thumbnailTypes[this.normalizeExtension(n)]
    }
    , getArchiveType: function (n) {
        return this.archiveTypes[this.normalizeExtension(n)]
    }
    , getImageViewerType: function (n) {
        var t = this.imageViewerTypes[this.normalizeExtension(n)];
        return t && t.excluded !== !0 ? t : null
    }
    , getMediaPlayerType: function (n) {
        return this.mediaPlayerTypes[this.normalizeExtension(n)]
    }
    , getDocumentViewerType: function (n) {
        var t = this.documentViewerTypes[this.normalizeExtension(n)];
        return t && t.excluded !== !0 ? t : null
    }
});
Ext.define("GleamTech.UI.MediaPlayer", {
    extend: "Ext.Component"
    , config: {
        libraryPath: ""
        , src: ""
        , format: ""
        , isAudio: !1
        , iconColor: "rgba(255, 255, 255, 1.0)"
        , hoverColor: "#3498db"
    }
    , minWidth: 320
    , minHeight: 240
    , onRender: function () {
        this.callParent(arguments);
        var n = Ext.Loader.config.disableCaching;
        Ext.Loader.setConfig({
            disableCaching: !1
        });
        Ext.Loader.loadScript({
            url: [GleamTech.Util.Path.combine(this.libraryPath, "jwplayer.js", GleamTech.Util.Path.forwardSlash), GleamTech.Util.Path.combine(this.libraryPath, "skins/tube.css", GleamTech.Util.Path.forwardSlash)]
            , onLoad: this.onLibraryLoaded
            , scope: this
        });
        Ext.Loader.setConfig({
            disableCaching: n
        })
    }
    , onLibraryLoaded: function () {
        this.videoEl = this.el.appendChild({
            tag: "div"
        });
        this.player = jwplayer(this.videoEl.id)
            .setup({
                file: this.src
                , type: this.format
                , image: GleamTech.Util.Path.combine(this.libraryPath, this.isAudio ? "skins/audio.png" : "skins/video.png", GleamTech.Util.Path.forwardSlash)
                , width: "100%"
                , height: "100%"
                , skin: {
                    controlbar: {
                        icons: this.iconColor
                        , iconsActive: this.hoverColor
                    }
                    , menus: {
                        textActive: this.hoverColor
                    }
                    , tooltips: {
                        text: this.hoverColor
                    }
                }
                , autostart: !0
                , forceLocalizationDefaults: !0
                , flashplayer: GleamTech.Util.Path.combine(this.libraryPath, "jwplayer.flash.swf", GleamTech.Util.Path.forwardSlash)
            });
        var n = this;
        this.player.on("meta", function (t) {
            var u = t.metadata || t
                , i = u.width
                , r = u.height;
            i < this.minWidth && (i = this.minWidth);
            r < this.minHeight && (r = this.minHeight);
            n.fireEvent("beforemediaresize", i, r);
            n.setSize(i, r);
            n.fireEvent("mediaresize", i, r)
        });
        this.player.on("setupError", function (t) {
            switch (t.code) {
                case 102630:
                    n.fireEvent("mediaformaterror", t.message);
                    break;
                default:
                    n.fireEvent("setuperror", t.message)
            }
        });
        this.player.on("error", function (t) {
            switch (t.code) {
                case 224003:
                    n.fireEvent("mediaformaterror", t.message);
                    break;
                default:
                    n.fireEvent("error", t.message)
            }
        })
    }
    , onDestroy: function () {
        this.player && this.player.remove();
        this.callParent(arguments)
    }
});
Ext.define("GleamTech.FileUltimate.FileManager", {
    extend: "GleamTech.UI.ExplorerView"
    , statics: {
        sprite: new GleamTech.UI.Sprite("filemanagericons")
    }
    , config: {
        sortRootFolders: !0
        , showFileExtensions: !1
        , chooser: !1
        , chooserType: "File"
        , chooserMultipleSelection: !1
        , downloadOnDoubleClick: !1
        , excludedExtensionsForPreview: ""
        , imageThumbnailsEnabled: !0
        , videoThumbnailsEnabled: !0
        , imageViewerEnabled: !0
        , mediaPlayerEnabled: !0
        , documentViewerEnabled: !0
        , archiveBrowsingEnabled: !0
        , resourceBasePath: ""
    }
    , downloadIframe: null
    , downloadForm: null
    , fileUploader: null
    , clipboard: null
    , chooseButton: null
    , childWindows: null
    , initComponent: function () {
        if (this.fireEventEx("loading"), this.clipboard = {
            isEmpty: !0
        }, this.childWindows = {}, this.excludedExtensionsForPreview = this.excludedExtensionsForPreview ? Ext.Array.toMap(this.excludedExtensionsForPreview.split(/,/g)) : {}, this.chooser && (this.viewMultipleSelection = this.chooserMultipleSelection), this.callParent(arguments), this.chooser) {
            this.chooserType = GleamTech.FileUltimate.FileManagerChooserType[this.chooserType];
            this.chooseButton = new Ext.button.Button(this.applyControlAction({
                enableConditions: {
                    itemTypes: function (n) {
                        switch (n.chooserType) {
                            case GleamTech.FileUltimate.FileManagerChooserType.Folder:
                                return {
                                    Folder: {
                                        multiple: n.chooserMultipleSelection
                                    }
                                };
                            case GleamTech.FileUltimate.FileManagerChooserType.FileOrFolder:
                                return {
                                    Folder: {
                                        multiple: n.chooserMultipleSelection
                                    }, File: {
                                        multiple: n.chooserMultipleSelection
                                    }
                                };
                            default:
                                return {
                                    File: {
                                        multiple: n.chooserMultipleSelection
                                    }
                                }
                        }
                    }(this)
                }
                , minWidth: 75
                , disabled: !0
            }, "Choose"));
            var n = new Ext.toolbar.Toolbar({
                ui: "footer"
                , region: "south"
                , layout: {
                    pack: "end"
                }
                , items: [this.chooseButton, {
                    text: GleamTech.Util.Language.getEntry("Label.Cancel")
                    , minWidth: 75
                    , handler: function () {
                        this.hide()
                    }
                    , scope: this
                }]
            });
            this.add(n);
            this.on("hide", function (n, t) {
                if (!this.hidingAfterChoose) this.onActionChoose(n, t)
            }, this)
        }
        this.setNavigationSelection(null);
        this.fireEventEx("loaded")
    }
    , afterRender: function () {
        this.downloadIframe = this.el.createChild({
            tag: "iframe"
            , name: Ext.id(null, "download-iframe-")
            , src: "javascript:&quot;&quot;"
            , style: "visibility: hidden; width: 0; height:0"
        });
        this.downloadForm = Ext.get(Ext.DomHelper.createDom({
            tag: "form"
            , style: "visibility: hidden; width: 0; height:0"
        }));
        Ext.isIE8 ? Ext.getBody()
            .appendChild(this.downloadForm) : this.el.appendChild(this.downloadForm);
        this.callParent(arguments)
    }
    , onBoxReady: function () {
        this.loadInitialFolder() || this.setNavigationSelection(this.navigationStore.getAt(0));
        this.callParent(arguments)
    }
    , initLabels: function () {
        this.callParent();
        this.emptyViewText = GleamTech.Util.Language.getEntry("Message.EmptyFolder");
        this.emptyNavigationViewText = GleamTech.Util.Language.getEntry("Message.NoRootFolders")
    }
    , initActions: function () {
        this.callParent();
        this.addControlActions([{
            actionName: "Cut"
            , languageKey: "Label.Cut.Verb"
            , iconName: "Cut"
        }, {
            actionName: "Copy"
            , languageKey: "Label.Copy.Verb"
            , iconName: "Copy"
        }, {
            actionName: "Paste"
            , languageKey: "Label.Paste.Verb"
            , iconName: "Paste"
        }, {
            actionName: "Delete"
            , languageKey: "Label.Delete"
            , iconName: "Delete"
        }, {
            actionName: "Rename"
            , languageKey: "Label.Rename"
            , iconName: "Rename"
        }, {
            actionName: "NewFolder"
            , languageKey: "Label.NewFolder"
            , iconName: "NewFolder"
        }, {
            actionName: "NewFile"
            , languageKey: "Label.NewFile"
            , iconName: "NewFile"
        }, {
            actionName: "Download"
            , languageKey: "Label.Download.Verb"
            , iconName: "Download"
        }, {
            actionName: "Preview"
            , languageKey: "Label.Preview.Verb"
            , iconName: "Preview"
        }, {
            actionName: "Upload"
            , languageKey: "Label.Upload.Verb"
            , iconName: "Upload"
        }, {
            actionName: "Open"
            , languageKey: "Label.Open.Verb"
            , iconName: "Open"
        }, {
            actionName: "AddToZip"
            , languageKey: "Label.AddToZip"
            , iconName: "AddToZip"
        }, {
            actionName: "ExtractAll"
            , languageKey: "Label.ExtractAll"
            , iconName: "ExtractAll"
        }, {
            actionName: "ExtractAllTo"
            , languageKey: "Label.ExtractAllTo"
        }, {
            actionName: "ExtractAllHere"
            , languageKey: "Label.ExtractAllHere"
        }, {
            actionName: "Choose"
            , languageKey: "Label.Choose"
        }], this.statics()
            .sprite)
    }
    , initStores: function () {
        var t = {
            model: "GleamTech.FileUltimate.FolderNodeModel"
            , proxy: this.getServerHandlerMethodProxy({
                methodInfoCallback: function (n) {
                    return {
                        name: "ExpandFolder"
                        , parameters: {
                            isRefresh: n.config.isRefresh == !0
                            , path: n.config.node.getPathData()
                                .fullPath
                        }
                    }
                }
                , readRecordsCallback: this.readFolderNodeRecords
                , scope: this
            })
            , clearOnLoad: !1
            , sorters: [{
                property: "itemType"
                , direction: "ASC"
            }, {
                property: "name"
                , direction: "ASC"
            }]
        }
            , i = {
                model: "GleamTech.FileUltimate.FolderViewModel"
                , proxy: this.getServerHandlerMethodProxy({
                    methodInfoCallback: function (n) {
                        return {
                            name: "ListFolder"
                            , parameters: {
                                isRefresh: n.config.isRefresh == !0
                                , path: n.config.node.getPathData().fullPath
                            }
                        }
                    }
                    , readRecordsCallback: this.readFolderViewRecords
                    , scope: this
                })
                , grouper: {
                    property: "itemType"
                    , direction: "ASC"
                }
                , sorters: [{
                    property: "name"
                    , direction: "ASC"
                }]
            }
            , n;
        this.callParent([t, i]);
        n = new Ext.util.SorterCollection;
        Ext.override(this.navigationStore, {
            fillNode: function (t, i) {
                for (var u = t.childNodes.length, r, f, e; u--;) r = t.childNodes[u], r.isExisting ? (r.data.expandable ? r.childNodes.length === 0 && r.set({
                    loaded: !1
                    , leaf: !1
                    , expandable: !0
                }) : r.removeAll(!1, !1, !0), delete r.isExisting) : t.removeChild(r);
                return f = this.sortOnLoad, this.sortOnLoad = !1, e = this.sorters, this.sorters = n, this.callParent(arguments), this.sortOnLoad = f, this.sorters = e, this.sortOnLoad && t.sort(null, !1, !0), t.childNodes.length === 0 ? t.set({
                    leaf: !0
                    , expanded: !1
                    , expandable: !1
                }) : (t.set({
                    leaf: !1
                    , expanded: !1
                    , expandable: !0
                }), t.expand()), i
            }
        });
        this.loadRootFolders()
    }
    , getViewColumnsConfig: function (n) {
        var f = this
            , u = {
                name: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Name")
                    , dataIndex: "name"
                    , formatterFn: function (n, t) {
                        return !f.showFileExtensions && t.data.itemType === GleamTech.FileUltimate.FileManagerItemType.File ? GleamTech.Util.Path.getFileNameWithoutExtension(n) : n
                    }
                    , hideable: !1
                    , flex: 1
                    , isPrimary: !0
                    , width: 272
                }
                , dateModified: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.DateModified")
                    , dataIndex: "dateModified"
                    , formatterFn: function (n) {
                        return GleamTech.Util.Culture.formatShortDateTime(n)
                    }
                    , isToolTipValue: !0
                    , width: 125
                }
                , type: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Type")
                    , dataIndex: "type"
                    , isTileFirstValue: !0
                    , tileFormatterFn: function (n, t) {
                        return t.data.itemType === GleamTech.FileUltimate.FileManagerItemType.File ? n : ""
                    }
                    , isToolTipValue: !0
                    , width: 125
                }
                , size: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Size")
                    , dataIndex: "size"
                    , align: "right"
                    , formatterFn: function (n) {
                        return GleamTech.Util.Culture.formatKBSize(n)
                    }
                    , isTileSecondValue: !0
                    , tileFormatterFn: function (n, t) {
                        return t.data.itemType === GleamTech.FileUltimate.FileManagerItemType.File ? GleamTech.Util.Culture.formatByteSize(n) : ""
                    }
                    , isToolTipValue: !0
                    , width: 85
                }
            }
            , i, t, r;
        if (n) {
            for (i = [], t = 0; t < n.length; t++) r = u[n[t]], r && i.push(r);
            return i
        }
        return Ext.Object.getValues(u)
    }
    , createRibbon: function () {
        var n = this.callParent()
            , t = n.items.getAt(0);
        return n.insertGroup(t, 0, {
            title: GleamTech.Util.Language.getEntry("Label.Clipboard")
            , items: [this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Copy"]
                        }
                        , Folder: {
                            permissions: ["Copy"]
                        }
                        , File: {
                            permissions: ["Copy"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        Folder: {
                            multiple: !0
                        }
                        , File: {
                            multiple: !0
                        }
                    }
                }
                , iconAlign: "top"
                , rowspan: 3
            }, "Copy", 32), this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Paste"]
                        }
                        , Folder: {
                            permissions: ["Paste"]
                        }
                        , File: {
                            permissions: ["Paste"]
                        }
                    }
                }
                , enableConditions: {
                    checkFn: function () {
                        return this.isNavigationSelectionValid && !this.clipboard.isEmpty
                    }
                    , scope: this
                }
                , iconAlign: "top"
                , rowspan: 3
            }, "Paste", 32), {
                xtype: "container"
                , layout: {
                    type: "vbox"
                    , align: "stretchmax"
                }
                , items: [this.applyControlAction({
                    showConditions: {
                        itemTypes: {
                            RootFolder: {
                                permissions: ["Cut"]
                            }
                            , Folder: {
                                permissions: ["Cut"]
                            }
                            , File: {
                                permissions: ["Cut"]
                            }
                        }
                    }
                    , enableConditions: {
                        itemTypes: {
                            Folder: {
                                multiple: !0
                            }
                            , File: {
                                multiple: !0
                            }
                        }
                    }
                }, "Cut", 16)]
            }]
        }), n.insertGroup(t, 1, {
            title: GleamTech.Util.Language.getEntry("Label.Organize")
            , items: [this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Delete"]
                        }
                        , Folder: {
                            permissions: ["Delete"]
                        }
                        , File: {
                            permissions: ["Delete"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        Folder: {
                            multiple: !0
                        }
                        , File: {
                            multiple: !0
                        }
                    }
                }
                , iconAlign: "top"
            }, "Delete", 32), this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Rename"]
                        }
                        , Folder: {
                            permissions: ["Rename"]
                        }
                        , File: {
                            permissions: ["Rename"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        Folder: {}
                        , File: {}
                    }
                }
                , iconAlign: "top"
            }, "Rename", 32)]
        }), n.insertGroup(t, 2, {
            title: GleamTech.Util.Language.getEntry("Label.New")
            , items: [this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Create"]
                        }
                        , Folder: {
                            permissions: ["Create"]
                        }
                        , File: {
                            permissions: ["Create"]
                        }
                    }
                }
                , enableConditions: {
                    checkFn: function () {
                        return this.isNavigationSelectionValid
                    }
                    , scope: this
                }
                , iconAlign: "top"
            }, "NewFolder", 32)]
        }), n.insertGroup(t, 3, {
            title: GleamTech.Util.Language.getEntry("Label.Transfer.Verb")
            , items: [this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Download"]
                        }
                        , Folder: {
                            permissions: ["Download"]
                        }
                        , File: {
                            permissions: ["Download"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        Folder: {
                            multiple: !0
                        }
                        , File: {
                            multiple: !0
                        }
                    }
                }
                , iconAlign: "top"
            }, "Download", 32), this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Upload"]
                        }
                        , Folder: {
                            permissions: ["Upload"]
                        }
                        , File: {
                            permissions: ["Upload"]
                        }
                    }
                }
                , enableConditions: {
                    checkFn: function () {
                        return this.isNavigationSelectionValid
                    }
                    , scope: this
                }
                , iconAlign: "top"
            }, "Upload", 32)]
        }), n.insertGroup(t, 4, {
            title: GleamTech.Util.Language.getEntry("Label.Open.Verb")
            , items: [this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Download", "ListSubfolders"]
                        }
                        , Folder: {
                            permissions: ["Download", "ListSubfolders"]
                        }
                        , File: {
                            permissions: ["Download", "ListSubfolders"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        Folder: {}
                        , File: {
                            parentPermissions: ["Download"]
                        }
                    }
                }
                , iconAlign: "top"
            }, "Open", 32), this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Preview"]
                        }
                        , Folder: {
                            permissions: ["Preview"]
                        }
                        , File: {
                            permissions: ["Preview"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        File: {
                            parentPermissions: ["Preview"]
                            , extensionCheckFn: this.isPreviewType
                            , scope: this
                        }
                    }
                }
                , iconAlign: "top"
            }, "Preview", 32)]
        }), n.insertGroup(t, 5, {
            title: GleamTech.Util.Language.getEntry("Label.Compression")
            , items: [this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Compress"]
                        }
                        , Folder: {
                            permissions: ["Compress"]
                        }
                        , File: {
                            permissions: ["Compress"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        Folder: {
                            multiple: !0
                        }
                        , File: {
                            multiple: !0
                        }
                    }
                }
                , iconAlign: "top"
            }, "AddToZip", 32), this.applyControlAction({
                showConditions: {
                    itemTypes: {
                        RootFolder: {
                            permissions: ["Extract"]
                        }
                        , Folder: {
                            permissions: ["Extract"]
                        }
                        , File: {
                            permissions: ["Extract"]
                        }
                    }
                }
                , enableConditions: {
                    itemTypes: {
                        File: {
                            extensionCheckFn: this.getArchiveType
                            , scope: this
                        }
                    }
                }
                , iconAlign: "top"
                , menu: {
                    items: [this.applyControlAction({}, "ExtractAllTo"), this.applyControlAction({}, "ExtractAllHere")]
                }
            }, "ExtractAll", 32)]
        }), Crypto.MD5(this.id + this.stateId) !== this.hash && n.tabBar.insert(n.tabBar.items.findIndex("xtype", "tbfill") + 1, {
            xtype: "component"
            , autoEl: {
                tag: "a"
                , href: "https://www.gleamtech.com/fileultimate/buy"
                , target: "_blank"
                , html: "FileUltimate (unlicensed) - Purchase a license"
                , style: {
                    color: "#000080"
                    , marginRight: "10px"
                    , lineHeight: "22px"
                }
            }
        }), n
    }
    , createNavigationPane: function () {
        var t = {
            viewConfig: {
                plugins: {
                    gttreedragdrop: {
                        getItemIconCls: {
                            fn: this.getItemIconCls
                            , scope: this
                        }
                        , enableDrag: !0
                        , enableDrop: !0
                        , ddGroup: "fileManager"
                    }
                    , gtviewnativedrop: {
                        enableNativeDrop: !0
                    }
                }
                , listeners: {
                    scope: this
                    , dragstart: this.onViewDragStart
                    , dropenter: this.onViewDropEnter
                    , dropout: this.onViewDropOut
                    , drop: this.onViewDrop
                    , containerdropover: this.onViewContainerDropOver
                    , containerdropout: this.onViewContainerDropOut
                    , containerdrop: this.onViewContainerDrop
                    , dropkeychange: this.onViewDropKeyChange
                    , nativedragenter: this.onViewNativeDragEnter
                    , nativedrop: this.onViewNativeDrop
                }
            }
        }
            , n = this.callParent([t]);
        return this.showFileExtensions || n.setColumns([{
            xtype: "treecolumn"
            , dataIndex: "name"
            , flex: 1
            , renderer: function (n, t, i) {
                return i.data.itemType === GleamTech.FileUltimate.FileManagerItemType.File ? GleamTech.Util.Path.getFileNameWithoutExtension(n) : n
            }
        }]), n
    }
    , createCenterPane: function () {
        var n = {
            enableDrag: !0
            , enableDrop: !0
            , ddGroup: "fileManager"
            , enableNativeDrop: !0
            , listeners: {
                scope: this
                , dragstart: this.onViewDragStart
                , dropenter: this.onViewDropEnter
                , dropout: this.onViewDropOut
                , drop: this.onViewDrop
                , containerdropover: this.onViewContainerDropOver
                , containerdropout: this.onViewContainerDropOut
                , containerdrop: this.onViewContainerDrop
                , dropkeychange: this.onViewDropKeyChange
                , nativedragenter: this.onViewNativeDragEnter
                , nativedrop: this.onViewNativeDrop
            }
        };
        return this.callParent([n])
    }
    , createNavigationViewItemContextMenu: function () {
        var n = this.callParent();
        return n.add([this.applyControlAction({
            showConditions: {
                checkFn: function (n) {
                    return !n.data.expanded
                }
            }
            , enableConditions: {
                checkFn: function (n) {
                    return n.data.expandable
                }
            }
        }, "Expand"), this.applyControlAction({
            showConditions: {
                checkFn: function (n) {
                    return n.data.expanded
                }
            }
        }, "Collapse"), this.applyControlAction({}, "Refresh", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({}, "Open", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Download"]
                    }
                    , File: {
                        parentPermissions: ["Download"]
                    }
                }
            }
        }, "Download", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Upload"]
                    }
                    , Folder: {
                        permissions: ["Upload"]
                    }
                    , File: {
                        permissions: ["Upload"]
                    }
                }
            }
        }, "Upload", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Compress"]
                    }
                    , File: {
                        parentPermissions: ["Compress"]
                    }
                }
            }
        }, "AddToZip", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    File: {
                        parentPermissions: ["Extract"]
                        , extensionCheckFn: this.getArchiveType
                        , scope: this
                    }
                }
            }
            , hideOnClick: !1
            , menu: [this.applyControlAction({}, "ExtractAllTo"), this.applyControlAction({}, "ExtractAllHere")]
        }, "ExtractAll", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Cut"]
                    }
                    , File: {
                        parentPermissions: ["Cut"]
                    }
                }
            }
        }, "Cut", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Copy"]
                    }
                    , File: {
                        parentPermissions: ["Copy"]
                    }
                }
            }
        }, "Copy", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Paste"]
                    }
                    , Folder: {
                        permissions: ["Paste"]
                    }
                    , File: {
                        permissions: ["Paste"]
                    }
                }
            }
            , enableConditions: {
                checkFn: function () {
                    return !this.clipboard.isEmpty
                }
                , scope: this
            }
        }, "Paste", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Delete"]
                    }
                    , File: {
                        parentPermissions: ["Delete"]
                    }
                }
            }
        }, "Delete", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Rename"]
                    }
                    , File: {
                        parentPermissions: ["Rename"]
                    }
                }
            }
        }, "Rename", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Create"]
                    }
                    , Folder: {
                        permissions: ["Create"]
                    }
                    , File: {
                        permissions: ["Create"]
                    }
                }
            }
        }, "NewFolder", 16)]), n
    }
    , createViewContainerContextMenu: function () {
        var n = this.callParent();
        return n.add([this.applyControlAction({
            showConditions: {
                checkFn: function () {
                    return this.isNavigationSelectionValid && !this.navigationSelection.isRoot()
                }
                , scope: this
            }
        }, "Refresh", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Upload"]
                    }
                    , Folder: {
                        permissions: ["Upload"]
                    }
                    , File: {
                        permissions: ["Upload"]
                    }
                }
            }
        }, "Upload", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Paste"]
                    }
                    , Folder: {
                        permissions: ["Paste"]
                    }
                    , File: {
                        permissions: ["Paste"]
                    }
                }
            }
            , enableConditions: {
                checkFn: function () {
                    return !this.clipboard.isEmpty
                }
                , scope: this
            }
        }, "Paste", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Create"]
                    }
                    , Folder: {
                        permissions: ["Create"]
                    }
                    , File: {
                        permissions: ["Create"]
                    }
                }
            }
        }, "NewFolder", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({}, "SelectAll", 16)]), n
    }
    , createViewItemContextMenu: function () {
        var n = this.callParent();
        return this.chooser && n.add([this.applyControlAction({
            enableConditions: {
                itemTypes: function (n) {
                    switch (n.chooserType) {
                        case GleamTech.FileUltimate.FileManagerChooserType.Folder:
                            return {
                                Folder: {
                                    multiple: n.chooserMultipleSelection
                                }
                            };
                        case GleamTech.FileUltimate.FileManagerChooserType.FileOrFolder:
                            return {
                                Folder: {
                                    multiple: n.chooserMultipleSelection
                                }, File: {
                                    multiple: n.chooserMultipleSelection
                                }
                            };
                        default:
                            return {
                                File: {
                                    multiple: n.chooserMultipleSelection
                                }
                            }
                    }
                }(this)
            }
        }, "Choose"), {
            xtype: "menuseparator"
        }]), n.add([this.applyControlAction({
            showConditions: {
                itemTypes: {
                    File: {
                        parentPermissions: ["Preview"]
                        , extensionCheckFn: this.isPreviewType
                        , scope: this
                    }
                }
            }
        }, "Preview", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {}
                    , Folder: {}
                    , File: {
                        parentPermissions: ["Download"]
                    }
                }
            }
        }, "Open", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Download"]
                        , multiple: !0
                    }
                    , File: {
                        parentPermissions: ["Download"]
                        , multiple: !0
                    }
                }
            }
        }, "Download", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Upload"]
                    }
                    , Folder: {
                        permissions: ["Upload"]
                    }
                    , File: {
                        permissions: ["Upload"]
                    }
                }
            }
        }, "Upload", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Compress"]
                        , multiple: !0
                    }
                    , File: {
                        parentPermissions: ["Compress"]
                        , multiple: !0
                    }
                }
            }
        }, "AddToZip", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    File: {
                        parentPermissions: ["Extract"]
                        , extensionCheckFn: this.getArchiveType
                        , scope: this
                    }
                }
            }
            , hideOnClick: !1
            , menu: [this.applyControlAction({}, "ExtractAllTo"), this.applyControlAction({}, "ExtractAllHere")]
        }, "ExtractAll", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Cut"]
                        , multiple: !0
                    }
                    , File: {
                        parentPermissions: ["Cut"]
                        , multiple: !0
                    }
                }
            }
        }, "Cut", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Copy"]
                        , multiple: !0
                    }
                    , File: {
                        parentPermissions: ["Copy"]
                        , multiple: !0
                    }
                }
            }
        }, "Copy", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Paste"]
                    }
                    , Folder: {
                        permissions: ["Paste"]
                    }
                    , File: {
                        permissions: ["Paste"]
                    }
                }
            }
            , enableConditions: {
                checkFn: function () {
                    return !this.clipboard.isEmpty
                }
                , scope: this
            }
        }, "Paste", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Delete"]
                        , multiple: !0
                    }
                    , File: {
                        parentPermissions: ["Delete"]
                        , multiple: !0
                    }
                }
            }
        }, "Delete", 16), this.applyControlAction({
            showConditions: {
                itemTypes: {
                    Folder: {
                        parentPermissions: ["Rename"]
                    }
                    , File: {
                        parentPermissions: ["Rename"]
                    }
                }
            }
        }, "Rename", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({
            showConditions: {
                itemTypes: {
                    RootFolder: {
                        permissions: ["Create"]
                    }
                    , Folder: {
                        permissions: ["Create"]
                    }
                }
            }
        }, "NewFolder", 16), {
            xtype: "menuseparator"
        }, this.applyControlAction({}, "InvertSelection", 16)]), n
    }
    , loadRootFolders: function () {
        this.navigationStore.sortOnLoad = this.sortRootFolders;
        var n = this.navigationStore.getRoot()
            , t = this.readFolderNodeRecords({
                config: {
                    node: n
                }
            }, {
                Folders: this.rootFolders
            });
        this.navigationStore.fillNode(n, t);
        this.navigationStore.sortOnLoad = !0
    }
    , loadInitialFolder: function () {
        var n, r, u, t;
        if (!this.initialFolder) return !1;
        if (n = this.initialFolder, n.charAt(0) === "[" && (t = n.indexOf("]:")) > -1) r = n.substr(1, t - 1), u = n.substr(t + 2)
            .split(GleamTech.Util.Path.backSlash);
        else if ((t = n.indexOf(":")) > -1) r = n.substr(0, t), u = n.substr(t + 1)
            .split(GleamTech.Util.Path.backSlash);
        else return !1;
        var e = [r].concat(Ext.Array.filter(u, function (n) {
            return n
        }))
            , i = this.navigationStore.getRoot()
            , f = 0
            , s = this
            , o = function () {
                (i = i.findChildNodeByName(e[f]), i) && (f === e.length - 1 ? s.setNavigationSelection(i) : (f++ , i.expand(!1, o)))
            };
        return o(), !0
    }
    , readFolderNodeRecords: function (n, t) {
        var o = n.config.node
            , s = t.Folders
            , h = s.length
            , r, u, f, i, e;
        for (o.isRoot() ? (r = GleamTech.FileUltimate.FileManagerItemType.RootFolder, u = GleamTech.FileUltimate.FileTypeInfo.getBaseIconCls("RootFolder", 16)) : (r = GleamTech.FileUltimate.FileManagerItemType.Folder, u = GleamTech.FileUltimate.FileTypeInfo.getBaseIconCls("Folder", 16)), f = [], i = 0; i < h; i++) e = this.createFolderNodeRecord(s[i], o, r, u), e != null && f.push(e);
        return f
    }
    , readFolderViewRecords: function (n, t) {
        for (var h, u, b, i = n.config.node, v = t.Folders, e = v.length, y = t.Files, p = y.length, r, o = new Array(e + p), s = [], f = 0; f < e; f++) h = v[f], o[f] = this.createFolderViewRecord(h, i, GleamTech.FileUltimate.FileManagerItemType.Folder), r = this.createFolderNodeRecord(h, i, GleamTech.FileUltimate.FileManagerItemType.Folder, GleamTech.FileUltimate.FileTypeInfo.getBaseIconCls("Folder", 16)), r != null && s.push(r);
        for (u = 0; u < p; u++) {
            var c = y[u]
                , l, w = GleamTech.Util.Path.getExtension(c[0], !0)
                , a = this.getArchiveBrowsingType(w);
            a && (l = !a.isReadOnly && i.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Edit) && i.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Compress) ? i.data.permissions : i.data.permissions & (GleamTech.FileUltimate.FileManagerPermissionTypes.ListSubfolders | GleamTech.FileUltimate.FileManagerPermissionTypes.ListFiles | GleamTech.FileUltimate.FileManagerPermissionTypes.Download | GleamTech.FileUltimate.FileManagerPermissionTypes.Preview | GleamTech.FileUltimate.FileManagerPermissionTypes.Copy), b = [c[0], l, null, !0], r = this.createFolderNodeRecord(b, i, GleamTech.FileUltimate.FileManagerItemType.File, GleamTech.FileUltimate.FileTypeInfo.getIconCls(w, 16)), r != null && s.push(r));
            r = o[e + u] = this.createFolderViewRecord(c, i, GleamTech.FileUltimate.FileManagerItemType.File);
            a && (r.data.permissions = l)
        }
        return n.config.folderNodeRecords = s, o
    }
    , createFolderNodeRecord: function (n, t, i, r) {
        var e = {
            itemType: i
            , name: n[0]
            , permissions: n[1] != null ? n[1] : t.data.permissions
            , fileTypes: n[2] != null ? n[2] : t.data.fileTypes
            , expandable: n[3]
            , hash: n[4]
            , iconCls: r
        }
            , u = t.findChildNodeByName(e.name)
            , f;
        return u ? (u.set(e), u.commit(), u.isExisting = !0, null) : (f = new GleamTech.FileUltimate.FolderNodeModel(e), f.phantom = !1, f.childrenNotLoadedYet = !0, f)
    }
    , createFolderViewRecord: function (n, t, i) {
        return i === GleamTech.FileUltimate.FileManagerItemType.Folder ? new GleamTech.FileUltimate.FolderViewModel({
            itemType: i
            , name: n[0]
            , permissions: n[1] != null ? n[1] : t.data.permissions
            , fileTypes: n[2] != null ? n[2] : t.data.fileTypes
            , size: null
            , systemType: n[4]
            , dateModified: n[5]
        }) : new GleamTech.FileUltimate.FolderViewModel({
            itemType: i
            , name: n[0]
            , size: n[1]
            , systemType: n[2]
            , dateModified: n[3]
        })
    }
    , getItemIconCls: function (n, t) {
        var i = n.data;
        switch (i.itemType) {
            case GleamTech.FileUltimate.FileManagerItemType.RootFolder:
                return GleamTech.FileUltimate.FileTypeInfo.getBaseIconCls("RootFolder", t);
            case GleamTech.FileUltimate.FileManagerItemType.Folder:
                return GleamTech.FileUltimate.FileTypeInfo.getBaseIconCls("Folder", t);
            case GleamTech.FileUltimate.FileManagerItemType.File:
                return GleamTech.FileUltimate.FileTypeInfo.getIconCls(i.extension, t);
            default:
                return ""
        }
    }
    , getItemThumbnailSrc: function (n, t) {
        var i = n.data;
        return i.itemType != GleamTech.FileUltimate.FileManagerItemType.File || i.size == 0 ? "" : this.getThumbnailType(i.extension) ? this.getServerHandlerMethodUrl("GetThumbnail", {
            stateId: this.stateId
            , path: this.navigationSelection.getPathData()
                .fullPath
            , fileName: i.name
            , maxSize: t
            , version: i.size + "-" + i.dateModified.getTime()
        }) : ""
    }
    , setNavigationSelection: function (n, t, i) {
        var o = this.navigationSelection
            , f, e, r, u, s;
        if (this.callParent([n]), this.isNavigationSelectionValid) {
            if (!i && this.navigationSelection != o)
                if (this.navigationSelection.isRoot()) {
                    for (f = this.navigationSelection.childNodes, e = [], r = 0; r < f.length; r++) u = f[r], s = new GleamTech.FileUltimate.FolderViewModel({
                        itemType: GleamTech.FileUltimate.FileManagerItemType.RootFolder
                        , name: u.data.name
                        , permissions: u.data.permissions
                        , fileTypes: u.data.fileTypes
                        , type: "Root Folder"
                    }), e.push(s);
                    this.viewStore.loadRecords(e);
                    this.fireEventEx("folderChanged", new GleamTech.UI.EventArgs({
                        folder: this.getEventFolder()
                    }))
                } else this.viewStore.load({
                    node: this.navigationSelection
                    , previousNode: o
                    , callback: t
                })
        } else this.ribbon && (this.ribbon.toggleItemsUICondition(this.getUIConditionFn(null, null, null, !1, !0)), this.ribbon.toggleItemsUICondition(this.getUIConditionFn(null, null, null, !0))), this.navigationBar.toggleItemsUICondition(this.getUIConditionFn(null, null, null, !0))
    }
    , onViewStoreDataChanged: function () {
        this.callParent(arguments);
        this.ribbon && (this.ribbon.toggleItemsUICondition(this.getUIConditionFn(this.navigationSelection, null, null, !1, !0)), this.ribbon.toggleItemsUICondition(this.getUIConditionFn(null, null, null, !0)));
        this.navigationBar && this.navigationBar.toggleItemsUICondition(this.getUIConditionFn(null, null, null, !0))
    }
    , onViewStoreLoad: function (n, t, i, r) {
        var u = r.config.node
            , f = r.config.previousNode
            , e = r.config.folderNodeRecords;
        i ? (this.navigationStore.fillNode(u, e), u.data.expanded = !1, u.expand(), r.config.isRefresh || this.fireEventEx("folderChanged", new GleamTech.UI.EventArgs({
            folder: this.getEventFolder()
        }))) : this.setNavigationSelection(f, null, !0);
        this.callParent(arguments)
    }
    , getSelectionStatusText: function (n) {
        var r = 0
            , u = n.length
            , t, i;
        if (u == 0) return "";
        for (t = 0; t < u; t++) {
            if (i = n[t].data, i.itemType != GleamTech.FileUltimate.FileManagerItemType.File) return "";
            r += i.size
        }
        return GleamTech.Util.Culture.formatByteSize(r)
    }
    , onViewSelectionChange: function (n, t) {
        this.callParent(arguments);
        var i = this.getUIConditionFn(t[t.length - 1], t, this.navigationSelection, !0);
        this.ribbon && this.ribbon.toggleItemsUICondition(i);
        this.chooseButton && this.chooseButton.toggleUICondition(i);
        this.fireEventEx("selectionChanged", new GleamTech.UI.EventArgs({
            items: this.getEventItems()
        }))
    }
    , onViewItemDblClick: function (n, t) {
        if (this.chooseButton && !this.chooseButton.disabled) {
            this.chooseButton.fireHandler();
            return
        }
        t.data.itemType === GleamTech.FileUltimate.FileManagerItemType.RootFolder || t.data.itemType === GleamTech.FileUltimate.FileManagerItemType.Folder || this.getArchiveBrowsingType(t.data.extension) ? this.setNavigationSelection(this.navigationSelection.findChildNodeByName(t.data.name)) : this.chooseButton || (this.downloadOnDoubleClick ? this.download(this.navigationSelection, t) : this.preview(this.navigationSelection, t) || this.downloadAndOpen(this.navigationSelection, t))
    }
    , onNavigationViewItemContextMenuBeforeShow: function (n) {
        n.toggleItemsUICondition(this.getUIConditionFn(this.contextMenuSelection, null, this.contextMenuSelection.parentNode))
    }
    , onViewContainerContextMenuBeforeShow: function (n) {
        n.toggleItemsUICondition(this.getUIConditionFn(this.navigationSelection))
    }
    , onViewItemContextMenuBeforeShow: function (n) {
        n.toggleItemsUICondition(this.getUIConditionFn(this.contextMenuSelection, this.viewSelection, this.navigationSelection))
    }
    , onViewDragStart: function (n, t, i) {
        var r, u;
        n.isTreeView ? (u = i.records[0], r = u.parentNode, i.copyAllowed = u.data.itemType !== GleamTech.FileUltimate.FileManagerItemType.RootFolder && r.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Copy), i.moveAllowed = u.data.itemType !== GleamTech.FileUltimate.FileManagerItemType.RootFolder && r.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Cut)) : (r = this.navigationSelection, i.copyAllowed = r.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Copy), i.moveAllowed = r.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Cut));
        i.copyAllowed || i.moveAllowed || t.proxy.setStatusNotAllowed()
    }
    , onViewDropEnter: function (n, t, i, r, u) {
        if (u.copyAllowed || u.moveAllowed) {
            var f = n.getRecord(i)
                , e = this.getViewDropAction(n, t, i, u, f);
            u.currentTarget = {
                target: i
                , record: f
                , action: e
            };
            this.setViewDropStatus(r.proxy, f, e)
        }
    }
    , onViewDropOut: function (n, t, i, r, u) {
        (u.copyAllowed || u.moveAllowed) && (delete u.currentTarget, r.proxy.setStatusHidden())
    }
    , onViewDropKeyChange: function (n, t, i, r) {
        if (r.copyAllowed || r.moveAllowed) {
            var f = r.currentTarget.record
                , u = this.getViewDropAction(n, t, r.currentTarget.target, r, f);
            u !== r.currentTarget.action && (this.setViewDropStatus(i.proxy, f, u), r.currentTarget.action = u)
        }
    }
    , onViewDrop: function (n, t, i, r, u) {
        return t.getKey() === t.ESC || !u.currentTarget || u.currentTarget.action !== GleamTech.FileUltimate.FileManagerDropAction.CopyTo && u.currentTarget.action !== GleamTech.FileUltimate.FileManagerDropAction.MoveTo ? !1 : (this.copyOrMove(u.currentTarget.action === GleamTech.FileUltimate.FileManagerDropAction.CopyTo ? GleamTech.FileUltimate.FileManagerClipboardAction.Copy : GleamTech.FileUltimate.FileManagerClipboardAction.Cut, u.view.isTreeView ? u.records[0].parentNode : this.navigationSelection, n.isTreeView ? u.currentTarget.record : u.currentTarget.target ? this.navigationSelection.findChildNodeByName(u.currentTarget.record.data.name) : this.navigationSelection, u.records), !0)
    }
    , onViewContainerDropOver: function (n, t, i, r) {
        if ((r.copyAllowed || r.moveAllowed) && !r.currentTarget) {
            var u = this.navigationSelection
                , f = this.getViewDropAction(n, t, null, r, u);
            r.currentTarget = {
                target: null
                , record: u
                , action: f
            };
            this.setViewDropStatus(i.proxy, u, f)
        }
    }
    , onViewContainerDropOut: function (n, t, i, r) {
        return this.onViewDropOut(n, t, null, i, r)
    }
    , onViewContainerDrop: function (n, t, i, r) {
        return this.onViewDrop(n, t, null, i, r)
    }
    , onViewNativeDragEnter: function (n, t) {
        var i;
        i = n.isTreeView ? t.record : t.record || this.navigationSelection;
        t.uploadAllowed = i && i.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Upload);
        t.uploadAllowed && t.hasFiles ? this.setViewDropStatus(t.proxy, i, GleamTech.FileUltimate.FileManagerDropAction.UploadTo) : t.proxy.setStatusNotAllowed()
    }
    , onViewNativeDragMove: function () { }
    , onViewNativeDragLeave: function () { }
    , onViewNativeDrop: function (n, t) {
        var r, i;
        t.uploadAllowed && t.hasFiles && (this.contextMenuSelection = t.record, r = n.isTreeView ? GleamTech.UI.ExplorerViewActionContext.NavigationViewItem : t.record ? GleamTech.UI.ExplorerViewActionContext.ViewItem : GleamTech.UI.ExplorerViewActionContext.View, i = this, GleamTech.FileUltimate.FileDropHelper.getFilesFromEvent(t.e.browserEvent, function (n) {
            i.onActionUpload(null, null, r, function (t) {
                t.addFiles(n, !0)
            })
        }, function (n) {
            i.messageBox.showWarning(n)
        }))
    }
    , setViewDropStatus: function (n, t, i) {
        switch (i) {
            case GleamTech.FileUltimate.FileManagerDropAction.Hidden:
                n.setStatusHidden();
                break;
            case GleamTech.FileUltimate.FileManagerDropAction.NotAllowed:
                n.setStatusNotAllowed();
                break;
            case GleamTech.FileUltimate.FileManagerDropAction.CopyTo:
                n.setStatusText(GleamTech.Util.Language.getEntry("Label.CopyTo", '<span class="x-dd-status-folder">' + t.data.name + "<\/span>"), "x-dd-status-copyto");
                break;
            case GleamTech.FileUltimate.FileManagerDropAction.MoveTo:
                n.setStatusText(GleamTech.Util.Language.getEntry("Label.MoveTo", '<span class="x-dd-status-folder">' + t.data.name + "<\/span>"), "x-dd-status-moveto");
                break;
            case GleamTech.FileUltimate.FileManagerDropAction.UploadTo:
                n.setStatusText(GleamTech.Util.Language.getEntry("Label.UploadTo", '<span class="x-dd-status-folder">' + t.data.name + "<\/span>"), "x-dd-status-uploadto")
        }
    }
    , getViewDropAction: function (n, t, i, r, u) {
        if (i) {
            if (i === r.item) return GleamTech.FileUltimate.FileManagerDropAction.Hidden;
            if (!u.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Paste)) return GleamTech.FileUltimate.FileManagerDropAction.NotAllowed
        } else if (n.isTreeView || !u.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Paste)) return GleamTech.FileUltimate.FileManagerDropAction.NotAllowed;
        return t.ctrlKey ? r.copyAllowed ? GleamTech.FileUltimate.FileManagerDropAction.CopyTo : GleamTech.FileUltimate.FileManagerDropAction.NotAllowed : t.shiftKey ? r.moveAllowed ? GleamTech.FileUltimate.FileManagerDropAction.MoveTo : GleamTech.FileUltimate.FileManagerDropAction.NotAllowed : r.moveAllowed ? !i && n === r.view ? GleamTech.FileUltimate.FileManagerDropAction.Hidden : GleamTech.FileUltimate.FileManagerDropAction.MoveTo : r.copyAllowed ? GleamTech.FileUltimate.FileManagerDropAction.Hidden : GleamTech.FileUltimate.FileManagerDropAction.NotAllowed
    }
    , getUIConditionFn: function (n, t, i, r, u) {
        var s, f, e, c, o, h;
        if (t && t.length > 1)
            for (f = {}, e = new GleamTech.FileUltimate.FolderItemModel, o = 0; o < t.length; o++) h = t[o], e.combinePermissionsOfRecord(h), f[h.data.itemType] = !0;
        else n && (s = Ext.Object.getKey(GleamTech.FileUltimate.FileManagerItemType, n.data.itemType), f = !1, n.data.itemType == GleamTech.FileUltimate.FileManagerItemType.File && (c = n.data.extension.toLowerCase()));
        return function (o, h, l) {
            var a, v, y, p, w;
            if (h && r || l && u || (v = o[h ? "showConditions" : "enableConditions"], !v)) return undefined;
            if (Ext.isFunction(v.checkFn)) return Ext.callback(v.checkFn, v.scope, [n, t, i]);
            if (f) {
                for (y in f)
                    if (p = Ext.Object.getKey(GleamTech.FileUltimate.FileManagerItemType, +y), a = v.itemTypes[p], !a || !a.multiple) return !1;
                for (w in v.itemTypes)
                    if ((a = v.itemTypes[w], a.parentPermissions && i && !i.checkEitherPermissionName(a.parentPermissions)) || a.permissions && e.hasPermissions() && !e.checkEitherPermissionName(a.permissions)) return !1;
                return !0
            }
            return s && v.itemTypes ? (a = v.itemTypes[s], !a) ? !1 : a.parentPermissions && i && !i.checkEitherPermissionName(a.parentPermissions) ? !1 : a.permissions && !n.checkEitherPermissionName(a.permissions) ? !1 : Ext.isFunction(a.extensionCheckFn) && !a.extensionCheckFn.apply(a.scope, [c]) ? !1 : !0 : !1
        }
    }
    , getThumbnailType: function (n) {
        var t = GleamTech.FileUltimate.FileTypeInfo.getThumbnailType(n);
        return t && (t.isImage && this.imageThumbnailsEnabled || t.isVideo && this.videoThumbnailsEnabled) ? t : null
    }
    , getArchiveType: function (n) {
        return GleamTech.FileUltimate.FileTypeInfo.getArchiveType(n)
    }
    , getArchiveBrowsingType: function (n) {
        return this.archiveBrowsingEnabled ? GleamTech.FileUltimate.FileTypeInfo.getArchiveType(n) : null
    }
    , getImageViewerType: function (n) {
        return this.imageViewerEnabled ? GleamTech.FileUltimate.FileTypeInfo.getImageViewerType(n) : null
    }
    , getMediaPlayerType: function (n) {
        return this.mediaPlayerEnabled ? GleamTech.FileUltimate.FileTypeInfo.getMediaPlayerType(n) : null
    }
    , getDocumentViewerType: function (n) {
        return this.documentViewerEnabled ? GleamTech.FileUltimate.FileTypeInfo.getDocumentViewerType(n) : null
    }
    , isPreviewType: function (n) {
        return (this.getImageViewerType(n) || this.getMediaPlayerType(n) || this.getDocumentViewerType(n)) && !this.excludedExtensionsForPreview[n.toLowerCase()]
    }
    , setClipboard: function (n, t, i) {
        this.clearClipboard();
        this.clipboard.action = n;
        this.clipboard.folderNodeRecord = t;
        this.clipboard.records = i;
        this.clipboard.isEmpty = !1;
        this.isNavigationSelectionValid && this.getControlAction("Paste")
            .enable()
    }
    , clearClipboard: function () {
        delete this.clipboard.action;
        delete this.clipboard.folderNodeRecord;
        delete this.clipboard.records;
        this.clipboard.isEmpty = !0;
        this.getControlAction("Paste")
            .disable()
    }
    , refreshFolderNode: function (n, t) {
        n == this.navigationSelection ? this.viewStore.load({
            node: n
            , isRefresh: !0
            , callback: t
        }) : this.navigationStore.load({
            node: n
            , isRefresh: !0
            , callback: t
        })
    }
    , download: function (n, t) {
        if (Ext.is.iOS) return this.downloadAndOpen(n, t);
        if (!n.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Download)) return !1;
        var i = this.fireEventEx("downloading", new GleamTech.UI.EventArgs({
            items: this.getEventItems(n, [t])
            , downloadFileName: t.data.name
            , openInBrowser: !1
        }));
        if (i !== !1) {
            while (this.downloadForm.dom.firstChild) this.downloadForm.dom.removeChild(this.downloadForm.dom.firstChild);
            return this.downloadForm.set({
                method: "get"
                , target: this.downloadIframe.dom.name
                , action: this.getServerHandlerMethodUrl("Download/" + GleamTech.Util.Path.getSafeFileNameForUrl(t.data.name))
            }), Ext.Object.each({
                stateId: this.stateId
                , path: n.getPathData()
                    .fullPath
                , openInBrowser: !1
                , fileName: t.data.name
            }, function (n, t) {
                this.downloadForm.createChild({
                    tag: "input"
                    , name: n
                    , type: "hidden"
                })
                    .dom.value = t
            }, this), this.downloadForm.dom.submit(), !0
        }
    }
    , downloadAndOpen: function (n, t) {
        var u;
        if (!n.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Download)) return !1;
        if (u = this.fireEventEx("downloading", new GleamTech.UI.EventArgs({
            items: this.getEventItems(n, [t])
            , downloadFileName: t.data.name
            , openInBrowser: !0
        })), u !== !1) {
            while (this.downloadForm.dom.firstChild) this.downloadForm.dom.removeChild(this.downloadForm.dom.firstChild);
            var r = n.getPathData()
                , i = t.data.name
                , f = t.data.extension.toLowerCase();
            return f === "htm" || f === "html" ? this.downloadForm.set({
                method: "get"
                , action: this.getServerHandlerMethodUrl(["Download", this.stateId, Ext.Array.map(r.parts, function (n) {
                    return GleamTech.Util.Path.getSafeFileNameForUrl(n)
                })
                    .join("/"), GleamTech.Util.Path.getSafeFileNameForUrl(i)].join("/"))
            }) : (this.downloadForm.set({
                method: "get"
                , action: this.getServerHandlerMethodUrl("Download/" + GleamTech.Util.Path.getSafeFileNameForUrl(i))
            }), Ext.Object.each({
                stateId: this.stateId
                , path: r.fullPath
                , openInBrowser: !0
                , fileName: i
            }, function (n, t) {
                this.downloadForm.createChild({
                    tag: "input"
                    , name: n
                    , type: "hidden"
                })
                    .dom.value = t
            }, this)), Ext.is.iOS ? (this.downloadForm.set({
                target: ""
            }), this.downloadForm.dom.submit()) : this.createChildWindow({
                childWindowId: "open-" + GleamTech.Util.Path.combine(r.fullPath, i, GleamTech.Util.Path.backSlash)
                , title: i
                , items: {
                    xtype: "component"
                    , autoEl: {
                        tag: "iframe"
                        , name: Ext.id(null, "file-iframe-")
                        , src: "javascript:&quot;&quot;"
                        , style: {
                            border: "none"
                            , backgroundColor: "white"
                        }
                        , frameBorder: "0"
                    }
                }
                , listeners: {
                    afterRender: {
                        fn: function (n) {
                            var i = n.el.query("iframe", !1)[0]
                                , t = i.dom.contentWindow || window.frames[i.dom.name]
                                , r;
                            t.document.open();
                            t.document.write('<div id="empty"><\/div>');
                            t.document.close();
                            r = Ext.Function.defer(function () {
                                var i;
                                try {
                                    i = t.document.getElementById("empty")
                                } catch (r) { }
                                i && n.close()
                            }, 3e3);
                            i.on("load", function () {
                                clearTimeout(r);
                                var i, u;
                                try {
                                    i = t.document.title;
                                    u = t.document.getElementById("empty")
                                } catch (f) { } (i === "Download Error" || u) && n.close()
                            });
                            this.downloadForm.set({
                                target: i.dom.name
                            });
                            this.downloadForm.dom.submit()
                        }
                        , scope: this
                    }
                }
            }), !0
        }
    }
    , downloadAsZip: function (n, t, i) {
        if (!n.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Download)) return !1;
        var r = this.fireEventEx("downloading", new GleamTech.UI.EventArgs({
            items: this.getEventItems(n, t)
            , downloadFileName: GleamTech.Util.Path.getFileNameWithoutExtension(i.data.name) + ".zip"
            , openInBrowser: !1
        }));
        if (r !== !1) {
            while (this.downloadForm.dom.firstChild) this.downloadForm.dom.removeChild(this.downloadForm.dom.firstChild);
            return this.downloadForm.set({
                method: "post"
                , target: this.downloadIframe.dom.name
                , action: this.getServerHandlerMethodUrl("DownloadAsZip")
            }), this.downloadForm.createChild({
                tag: "input"
                , name: "parameters"
                , type: "hidden"
            })
                .dom.value = Ext.encode({
                    stateId: this.stateId
                    , path: n.getPathData()
                        .fullPath
                    , itemNames: this.getItemNamesWithType(t)
                    , zipFileName: GleamTech.Util.Path.getFileNameWithoutExtension(i.data.name) + ".zip"
                }), this.downloadForm.dom.submit(), !0
        }
    }
    , preview: function (n, t) {
        var i;
        if (!n.checkPermission(GleamTech.FileUltimate.FileManagerPermissionTypes.Preview)) return !1;
        var u = n.getPathData()
            , r = t.data.name
            , e = t.data.extension
            , o, f;
        if (this.excludedExtensionsForPreview[e.toLowerCase()]) return !1;
        if (this.getImageViewerType(e)) {
            if (f = this.fireEventEx("previewing", new GleamTech.UI.EventArgs({
                item: this.getEventItems(n, [t])[0]
                , previewerType: "ImageViewer"
            })), f === !1) return;
            this.createChildWindow({
                childWindowId: "preview-" + GleamTech.Util.Path.combine(u.fullPath, r, GleamTech.Util.Path.backSlash)
                , title: r + " - " + GleamTech.Util.Language.getEntry("Label.ImageViewer")
                , minWidth: 320
                , minHeight: 240
                , items: [{
                    xtype: "box"
                    , autoEl: {
                        tag: "iframe"
                        , frameBorder: "0"
                        , allowFullScreen: !0
                        , webkitallowfullscreen: !0
                        , mozallowfullscreen: !0
                        , src: this.getServerHandlerMethodUrl("Preview", {
                            stateId: this.stateId
                            , path: u.fullPath
                            , fileName: r
                            , previewerType: "ImageViewer"
                        })
                    }
                }]
            })
        } else if (o = this.getMediaPlayerType(e)) {
            if (f = this.fireEventEx("previewing", new GleamTech.UI.EventArgs({
                item: this.getEventItems(n, [t])[0]
                , previewerType: "MediaPlayer"
            })), f === !1) return;
            i = this.createChildWindow({
                childWindowId: "preview-" + GleamTech.Util.Path.combine(u.fullPath, r, GleamTech.Util.Path.backSlash)
                , title: r + " - " + GleamTech.Util.Language.getEntry("Label.MediaPlayer")
                , shrinkWrap: 3
                , width: undefined
                , height: undefined
                , items: new GleamTech.UI.MediaPlayer({
                    libraryPath: GleamTech.Util.Path.combine(this.resourceBasePath, "library/jwp")
                    , src: this.getServerHandlerMethodUrl("Preview/" + GleamTech.Util.Path.getSafeFileNameForUrl(r), {
                        stateId: this.stateId
                        , path: u.fullPath
                        , fileName: r
                        , previewerType: "MediaPlayer"
                    })
                    , format: o.format || e.toLowerCase()
                    , isAudio: o.isAudio
                    , listeners: {
                        scope: this
                        , beforemediaresize: function () {
                            i.minWidth = i.getWidth();
                            i.minHeight = i.getHeight();
                            i.maxWidth = this.getWidth() * 90 / 100;
                            i.maxHeight = this.getHeight() * 90 / 100
                        }
                        , mediaresize: function () {
                            i.width = i.getWidth();
                            i.height = i.getHeight();
                            i.maxWidth = i.maxHeight = undefined;
                            i.center()
                        }
                        , mediaformaterror: function () {
                            this.messageBox.showConfirm({
                                title: GleamTech.Util.Language.getEntry("Label.MediaFormatError")
                                , message: GleamTech.Util.Language.getEntry("Message.Error.MediaFormatError")
                                , fn: function (r) {
                                    r === "yes" && (i.close(), this.downloadAndOpen(n, t))
                                }
                                , scope: this
                            })
                        }
                    }
                })
            })
        } else if (this.getDocumentViewerType(e)) {
            if (f = this.fireEventEx("previewing", new GleamTech.UI.EventArgs({
                item: this.getEventItems(n, [t])[0]
                , previewerType: "DocumentViewer"
            })), f === !1) return;
            this.createChildWindow({
                childWindowId: "preview-" + GleamTech.Util.Path.combine(u.fullPath, r, GleamTech.Util.Path.backSlash)
                , title: r + " - " + GleamTech.Util.Language.getEntry("Label.DocumentViewer")
                , items: [{
                    xtype: "box"
                    , autoEl: {
                        tag: "iframe"
                        , frameBorder: "0"
                        , allowFullScreen: !0
                        , webkitallowfullscreen: !0
                        , mozallowfullscreen: !0
                        , src: this.getServerHandlerMethodUrl("Preview", {
                            stateId: this.stateId
                            , path: u.fullPath
                            , fileName: r
                            , previewerType: "DocumentViewer"
                        })
                    }
                }]
            })
        } else return !1;
        return !0
    }
    , createChildWindow: function (n) {
        var t;
        if (t = this.childWindows[n.childWindowId]) t.restoreMinimize();
        else if (t = new GleamTech.UI.Window(Ext.apply({
            layout: "fit"
            , width: this.getWidth() * 90 / 100
            , height: this.getHeight() * 90 / 100
            , minimizeOffset: [0, this.statusBar ? this.statusBar.getHeight() * -1 : 0]
            , resizable: !0
            , maximizable: !0
            , minimizable: !0
        }, n)), this.add(t), n.childWindowId) {
            this.childWindows[n.childWindowId] = t;
            t.on("close", function (n) {
                delete this.childWindows[n.childWindowId]
            }, this)
        }
        return t.show(), t
    }
    , getItemNameWithType: function (n) {
        return n.data.itemType == GleamTech.FileUltimate.FileManagerItemType.Folder ? n.data.name + "\\" : n.data.name
    }
    , getItemNamesWithType: function (n) {
        return Ext.Array.map(n, function (n) {
            return this.getItemNameWithType(n)
        }, this)
    }
    , getItemNameAsFolder: function (n) {
        return n + "\\"
    }
    , getEventFolder: function (n) {
        return (n = n || this.navigationSelection, !n) ? null : n.isRoot() ? {
            itemType: "Home"
            , name: ""
            , fullPath: ""
        } : {
                itemType: Ext.Object.getKey(GleamTech.FileUltimate.FileManagerItemType, n.data.itemType)
                , name: n.data.name
                , fullPath: n.getPathData()
                    .fullPath
            }
    }
    , getEventItems: function (n, t) {
        var f, u, r, i;
        for (n = n || this.navigationSelection, t = t || this.viewSelection, f = n.getPathData()
            .fullPath, u = [], r = 0; r < t.length; r++) i = t[r], u.push({
                itemType: Ext.Object.getKey(GleamTech.FileUltimate.FileManagerItemType, i.data.itemType)
                , name: i.data.name
                , fullPath: GleamTech.Util.Path.combine(f, i.data.name, GleamTech.Util.Path.backSlash)
                , extension: i.data.extension
                , dateModified: i.data.dateModified
                , type: i.data.type
                , size: i.data.size
            });
        return u
    }
    , onCancelEvent: function (n) {
        n.cancelMessage && this.messageBox.showError({
            title: GleamTech.Util.Language.getEntry("Label.ActionCanceled")
            , message: n.cancelMessage
        })
    }
    , onActionRefresh: function (n, t, i) {
        var r;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
                r = this.navigationSelection;
                break;
            default:
                return
        }
        this.refreshFolderNode(r)
    }
    , onActionCut: function (n, t, i) {
        var r, u;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection.parentNode;
                u = [this.contextMenuSelection];
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection;
                u = this.viewSelection;
                break;
            default:
                return
        }
        this.setClipboard(GleamTech.FileUltimate.FileManagerClipboardAction.Cut, r, u)
    }
    , onActionCopy: function (n, t, i) {
        var r, u;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection.parentNode;
                u = [this.contextMenuSelection];
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection;
                u = this.viewSelection;
                break;
            default:
                return
        }
        this.setClipboard(GleamTech.FileUltimate.FileManagerClipboardAction.Copy, r, u)
    }
    , onActionPaste: function (n, t, i) {
        var r, u;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
                r = this.navigationSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection.findChildNodeByName(this.contextMenuSelection.data.name);
                break;
            default:
                return
        }
        if (u = this.clipboard.folderNodeRecord, !u.parentNode) {
            this.clearClipboard();
            return
        }
        this.copyOrMove(this.clipboard.action, u, r, this.clipboard.records);
        this.clipboard.action == GleamTech.FileUltimate.FileManagerClipboardAction.Cut && this.clearClipboard()
    }
    , copyOrMove: function (n, t, i, r) {
        var u = !1
            , f;
        (n == GleamTech.FileUltimate.FileManagerClipboardAction.Cut && Ext.Array.each(r, function (n) {
            var i = t.findChildNodeByName(n.data.name);
            return i && (i == this.navigationSelection || i.contains(this.navigationSelection)) ? (u = !0, !1) : !0
        }, this), f = this.fireEventEx(n === GleamTech.FileUltimate.FileManagerClipboardAction.Cut ? "moving" : "copying", new GleamTech.UI.EventArgs({
            items: this.getEventItems(t, r)
            , targetFolder: this.getEventFolder(i)
        })), f !== !1) && this.callServerHandlerMethod({
            name: n == GleamTech.FileUltimate.FileManagerClipboardAction.Cut ? "Move" : "Copy"
            , parameters: {
                path: t.getPathData()
                    .fullPath
                , itemNames: this.getItemNamesWithType(r)
                , targetPath: i.getPathData()
                    .fullPath
            }
            , callback: function (f, e) {
                if (!e) {
                    this.fireEventEx(n === GleamTech.FileUltimate.FileManagerClipboardAction.Cut ? "moved" : "copied", new GleamTech.UI.EventArgs({
                        items: this.getEventItems(t, r)
                        , targetFolder: this.getEventFolder(i)
                    }));
                    var s = this
                        , o = null;
                    i != t && (o = function () {
                        i.parentNode && s.refreshFolderNode(i)
                    });
                    u ? this.setNavigationSelection(t, o) : this.refreshFolderNode(t, o)
                }
            }
            , scope: this
        })
    }
    , onActionDelete: function (n, t, i) {
        var u, r, o = !1
            , f, e, s;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                u = this.contextMenuSelection.parentNode;
                r = [this.contextMenuSelection];
                (this.contextMenuSelection === this.navigationSelection || this.contextMenuSelection.contains(this.navigationSelection)) && (o = !0);
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                u = this.navigationSelection;
                r = this.viewSelection;
                break;
            default:
                return
        }
        r.length === 1 ? (s = r[0], s.data.itemType === GleamTech.FileUltimate.FileManagerItemType.Folder ? (f = GleamTech.Util.Language.getEntry("Label.DeleteFolder"), e = GleamTech.Util.Language.getEntry("Message.Confirm.DeleteFolder")) : (f = GleamTech.Util.Language.getEntry("Label.DeleteFile"), e = GleamTech.Util.Language.getEntry("Message.Confirm.DeleteFile"))) : (f = GleamTech.Util.Language.getEntry("Label.DeleteMultipleItems"), e = GleamTech.Util.Language.getEntry("Message.Confirm.DeleteMultipleItems", r.length));
        this.messageBox.showConfirm({
            title: f
            , message: e
            , fn: function (n) {
                if (n === "yes") {
                    var t = this.fireEventEx("deleting", new GleamTech.UI.EventArgs({
                        items: this.getEventItems(u, r)
                    }));
                    t !== !1 && this.callServerHandlerMethod({
                        name: "Delete"
                        , parameters: {
                            path: u.getPathData()
                                .fullPath
                            , itemNames: this.getItemNamesWithType(r)
                        }
                        , callback: function (n, t) {
                            t || (this.fireEventEx("deleted", new GleamTech.UI.EventArgs({
                                items: this.getEventItems(u, r)
                            })), o ? this.setNavigationSelection(u) : this.refreshFolderNode(u))
                        }
                        , scope: this
                    })
                }
            }
            , scope: this
            , itemsConfig: {
                records: r
                , model: "GleamTech.FileUltimate.FolderViewModel"
                , multiViewConfig: {
                    viewLayout: this.centerPane.viewLayout.name
                    , detailsLayoutThreshold: this.viewDetailsLayoutThreshold
                    , columns: this.getViewColumnsConfig(["name", "dateModified", "type", "size"])
                    , getItemIconCls: {
                        fn: this.getItemIconCls
                        , scope: this
                    }
                    , getItemThumbnailSrc: {
                        fn: this.getItemThumbnailSrc
                        , scope: this
                    }
                }
            }
        })
    }
    , onActionRename: function (n, t, i) {
        var r, u, f, c, o, e, s, h;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection.parentNode;
                u = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection;
                u = this.viewSelection[0];
                break;
            default:
                return
        }
        f = u.data.name;
        u.data.itemType == GleamTech.FileUltimate.FileManagerItemType.File ? (c = GleamTech.Util.Path.getExtension(f), o = GleamTech.Util.Path.getFileNameWithoutExtension(f), e = this.showFileExtensions ? f : o, s = !this.showFileExtensions, h = o.length) : (e = f, s = !1, h = f.length);
        this.messageBox.showPrompt({
            title: GleamTech.Util.Language.getEntry("Label.RenameItem")
            , message: GleamTech.Util.Language.getEntry("Message.Prompt.RenameItem")
            , fn: function (n, t) {
                var i, e;
                n == "ok" && (i = t, s && (i += c), e = this.fireEventEx("renaming", new GleamTech.UI.EventArgs({
                    item: this.getEventItems(r, [u])[0]
                    , itemNewName: i
                })), e !== !1) && this.callServerHandlerMethod({
                    name: "Rename"
                    , parameters: {
                        path: r.getPathData()
                            .fullPath
                        , itemName: this.getItemNameWithType(u)
                        , itemNewName: i
                    }
                    , callback: function (n, t) {
                        if (!t) {
                            this.fireEventEx("renamed", new GleamTech.UI.EventArgs({
                                item: this.getEventItems(r, [u])[0]
                                , itemNewName: i
                            }));
                            var e = r.findChildNodeByName(f);
                            e && (e.set("name", i), e.commit());
                            this.refreshFolderNode(r)
                        }
                    }
                    , scope: this
                })
            }
            , scope: this
            , value: e
            , promptConfig: {
                selectionLength: h
                , validatorFn: function (n) {
                    return n == e || Ext.String.trim(n) == "" ? !1 : GleamTech.Util.Path.isValidFileName(n) ? !0 : GleamTech.Util.Language.getEntry("Message.Error.InvalidFileName") + '<br/>&emsp;&emsp;&emsp; \\ / : * ? " < > |'
                }
            }
        })
    }
    , onActionNewFolder: function (n, t, i) {
        var r, u;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
                r = this.navigationSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection.findChildNodeByName(this.contextMenuSelection.data.name);
                break;
            default:
                return
        }
        u = GleamTech.Util.Language.getEntry("Label.NewFolder");
        this.messageBox.showPrompt({
            title: GleamTech.Util.Language.getEntry("Label.CreateFolder")
            , message: GleamTech.Util.Language.getEntry("Message.Prompt.CreateFolder")
            , fn: function (n, t) {
                if (n == "ok") {
                    var i = this.fireEventEx("creating", new GleamTech.UI.EventArgs({
                        itemName: t
                        , itemType: "Folder"
                        , targetFolder: this.getEventFolder(r)
                    }));
                    i !== !1 && this.callServerHandlerMethod({
                        name: "Create"
                        , parameters: {
                            path: r.getPathData()
                                .fullPath
                            , itemName: this.getItemNameAsFolder(t)
                        }
                        , callback: function (n, i) {
                            i || (this.fireEventEx("created", new GleamTech.UI.EventArgs({
                                itemName: t
                                , itemType: "Folder"
                                , targetFolder: this.getEventFolder(r)
                            })), this.refreshFolderNode(r))
                        }
                        , scope: this
                    })
                }
            }
            , scope: this
            , value: u
            , promptConfig: {
                selectionLength: u.length
                , validatorFn: function (n) {
                    return Ext.String.trim(n) == "" ? !1 : GleamTech.Util.Path.isValidFileName(n) ? !0 : GleamTech.Util.Language.getEntry("Message.Error.InvalidFileName") + '<br/>&emsp;&emsp;&emsp; \\ / : * ? " < > |'
                }
            }
        })
    }
    , onActionDownload: function (n, t, i) {
        var u, f, r;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                u = this.contextMenuSelection.parentNode;
                f = [this.contextMenuSelection];
                r = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
                u = this.navigationSelection;
                f = this.viewSelection;
                r = this.viewSelection[0];
                break;
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                u = this.navigationSelection;
                f = this.viewSelection;
                r = this.contextMenuSelection;
                break;
            default:
                return
        }
        f.length > 1 || r.data.itemType === GleamTech.FileUltimate.FileManagerItemType.Folder ? this.downloadAsZip(u, f, r) : this.download(u, r)
    }
    , onActionUpload: function (n, t, i, r) {
        var u;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                u = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
                u = this.navigationSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                u = this.navigationSelection.findChildNodeByName(this.contextMenuSelection.data.name);
                break;
            default:
                return
        }
        if (!this.fileUploader || this.fileUploader.isDestroyed) {
            this.fileUploader = GleamTech.UI.Loader.createWithOwner(this.fileUploaderConfig);
            this.add(this.fileUploader.ownerCt);
            this.fileUploader.on({
                uploading: function (n, t) {
                    var i = n.fileManagerData;
                    return t.targetFolder = this.getEventFolder(i.folderNodeRecord), this.fireEventEx("uploading", t)
                }
                , uploaded: function (n, t) {
                    var f = n.fileManagerData
                        , i, u, e, r, o;
                    if (f.folderNodeRecord === this.navigationSelection) {
                        for (i = [], u = 0; u < t.items.length; u++) e = t.items[u], e.status === "Completed" && (r = e.name, o = r.indexOf("\\"), o > 0 && (r = r.substr(0, o)), i.push(r));
                        if (i.length) {
                            this.viewStore.on("refresh", function () {
                                for (var u, t, f, r = [], n = 0; n < i.length; n++) u = i[n], t = this.viewStore.findBy(function (n) {
                                    return n.data.name === u
                                }), t > -1 && r.push(this.viewStore.getAt(t));
                                f = this.centerPane.getSelectionModel();
                                f.select(r)
                            }, this, {
                                single: !0
                            });
                            this.refreshFolderNode(f.folderNodeRecord)
                        }
                    }
                    t.targetFolder = this.getEventFolder(f.folderNodeRecord);
                    this.fireEventEx("uploaded", t)
                }
                , ready: function (n) {
                    var t = n.fileManagerData;
                    n.setFileTypes(t.folderNodeRecord.data.fileTypes);
                    n.setCustomParameters({
                        fileManagerStateId: this.stateId
                        , fileManagerPath: t.folderNodeRecord.getPathData()
                            .fullPath
                    });
                    t.readyCallback && t.readyCallback(n)
                }
                , scope: this
            })
        }
        this.fileUploader.setViewLayout(this.centerPane.viewLayout.name);
        this.fileUploader.fileManagerData = {
            folderNodeRecord: u
            , readyCallback: r
        };
        this.fileUploader.ownerCt.setTitle(GleamTech.Util.Language.getEntry("Label.UploadFiles") + " - " + u.data.name);
        this.fileUploader.ownerCt.show();
        this.fileUploader.ownerCt.center()
    }
    , onActionOpen: function (n, t, i) {
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                this.setNavigationSelection(this.contextMenuSelection);
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                var r = this.viewSelection[0];
                r.data.itemType === GleamTech.FileUltimate.FileManagerItemType.RootFolder || r.data.itemType === GleamTech.FileUltimate.FileManagerItemType.Folder || this.getArchiveBrowsingType(r.data.extension) ? this.setNavigationSelection(this.navigationSelection.findChildNodeByName(r.data.name)) : this.downloadAndOpen(this.navigationSelection, r);
                break;
            default:
                return
        }
    }
    , onActionPreview: function (n, t, i) {
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.View:
                this.preview(this.navigationSelection, this.viewSelection[0]);
                break;
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                this.preview(this.navigationSelection, this.contextMenuSelection);
                break;
            default:
                return
        }
    }
    , onActionAddToZip: function (n, t, i) {
        var r, u, f, s, e, h, c, l, o;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection.parentNode;
                u = [this.contextMenuSelection];
                f = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
                r = this.navigationSelection;
                u = this.viewSelection;
                f = this.viewSelection[0];
                break;
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection;
                u = this.viewSelection;
                f = this.contextMenuSelection;
                break;
            default:
                return
        }
        s = ".zip";
        e = f.data.name;
        f.data.itemType == GleamTech.FileUltimate.FileManagerItemType.File ? (o = GleamTech.Util.Path.getFileNameWithoutExtension(e), h = this.showFileExtensions ? o + s : o, c = !this.showFileExtensions, l = o.length) : (h = e, c = !0, l = e.length);
        this.messageBox.showPrompt({
            title: GleamTech.Util.Language.getEntry("Label.AddItemsToZip")
            , message: GleamTech.Util.Language.getEntry("Message.Prompt.AddItemsToZip")
            , fn: function (n, t) {
                var i, f;
                n == "ok" && (i = t, c && (i += s), f = this.fireEventEx("compressing", new GleamTech.UI.EventArgs({
                    items: this.getEventItems(r, u)
                    , archiveFileName: i
                    , targetFolder: this.getEventFolder(r)
                })), f !== !1) && this.callServerHandlerMethod({
                    name: "AddToZip"
                    , parameters: {
                        path: r.getPathData()
                            .fullPath
                        , itemNames: this.getItemNamesWithType(u)
                        , zipFileName: i
                    }
                    , callback: function (n, t) {
                        t || (this.fireEventEx("compressed", new GleamTech.UI.EventArgs({
                            items: this.getEventItems(r, u)
                            , archiveFileName: i
                            , targetFolder: this.getEventFolder(r)
                        })), this.refreshFolderNode(r))
                    }
                    , timeout: 36e5
                    , scope: this
                })
            }
            , scope: this
            , value: h
            , promptConfig: {
                selectionLength: l
                , validatorFn: function (n) {
                    return Ext.String.trim(n) == "" ? !1 : GleamTech.Util.Path.isValidFileName(n) ? !0 : GleamTech.Util.Language.getEntry("Message.Error.InvalidFileName") + '<br/>&emsp;&emsp;&emsp; \\ / : * ? " < > |'
                }
            }
        })
    }
    , onActionExtractAllTo: function (n, t, i) {
        var r, u, f, e;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection.parentNode;
                u = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection;
                u = this.viewSelection[0];
                break;
            default:
                return
        }
        f = u.data.name;
        e = GleamTech.Util.Path.getFileNameWithoutExtension(f);
        this.messageBox.showPrompt({
            title: GleamTech.Util.Language.getEntry("Label.ExtractItemsFromArchive")
            , message: GleamTech.Util.Language.getEntry("Message.Prompt.ExtractItemsFromArchive")
            , fn: function (n, t) {
                if (n == "ok") {
                    var i = r.getPathData()
                        .fullPath
                        , e = this.fireEventEx("extracting", new GleamTech.UI.EventArgs({
                            item: this.getEventItems(r, [u])[0]
                            , targetFolder: this.getEventFolder(r)
                            , targetSubfolderName: t
                        }));
                    e !== !1 && this.callServerHandlerMethod({
                        name: "ExtractAll"
                        , parameters: {
                            path: i
                            , archiveFileName: f
                            , targetPath: i
                            , folderName: t
                        }
                        , callback: function (n, i) {
                            i || (this.fireEventEx("extracted", new GleamTech.UI.EventArgs({
                                item: this.getEventItems(r, [u])[0]
                                , targetFolder: this.getEventFolder(r)
                                , targetSubfolderName: t
                            })), this.refreshFolderNode(r))
                        }
                        , timeout: 36e5
                        , scope: this
                    })
                }
            }
            , scope: this
            , value: e
            , promptConfig: {
                selectionLength: e.length
                , validatorFn: function (n) {
                    return Ext.String.trim(n) == "" ? !1 : GleamTech.Util.Path.isValidFileName(n) ? !0 : GleamTech.Util.Language.getEntry("Message.Error.InvalidFileName") + '<br/>&emsp;&emsp;&emsp; \\ / : * ? " < > |'
                }
            }
        })
    }
    , onActionExtractAllHere: function (n, t, i) {
        var r, u;
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.NavigationViewItem:
                r = this.contextMenuSelection.parentNode;
                u = this.contextMenuSelection;
                break;
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                r = this.navigationSelection;
                u = this.viewSelection[0];
                break;
            default:
                return
        }
        var e = u.data.name
            , f = r.getPathData()
                .fullPath
            , o = this.fireEventEx("extracting", new GleamTech.UI.EventArgs({
                item: this.getEventItems(r, [u])[0]
                , targetFolder: this.getEventFolder(r)
                , targetSubfolderName: ""
            }));
        o !== !1 && this.callServerHandlerMethod({
            name: "ExtractAll"
            , parameters: {
                path: f
                , archiveFileName: e
                , targetPath: f
                , folderName: ""
            }
            , callback: function (n, t) {
                t || (this.fireEventEx("extracted", new GleamTech.UI.EventArgs({
                    item: this.getEventItems(r, [u])[0]
                    , targetFolder: this.getEventFolder(r)
                    , targetSubfolderName: ""
                })), this.refreshFolderNode(r))
            }
            , timeout: 36e5
            , scope: this
        })
    }
    , onActionChoose: function (n, t, i) {
        switch (i) {
            case GleamTech.UI.ExplorerViewActionContext.View:
            case GleamTech.UI.ExplorerViewActionContext.ViewItem:
                this.hidingAfterChoose = !0;
                this.hide();
                delete this.hidingAfterChoose;
                this.fireEventEx("chosen", new GleamTech.UI.EventArgs({
                    isCanceled: !1
                    , items: this.getEventItems()
                }));
                break;
            default:
                this.fireEventEx("chosen", new GleamTech.UI.EventArgs({
                    isCanceled: !0
                    , items: []
                }))
        }
        this.centerPane.getSelectionModel()
            .deselectAll()
    }
});
Ext.define("GleamTech.FileUltimate.FolderItemModel", {
    extend: "Ext.data.Model"
    , fields: [{
        name: "itemType"
    }, {
        name: "name"
        , sortType: "asLocaleString"
    }, {
        name: "permissions"
    }, {
        name: "fileTypes"
    }, {
        name: "extension"
        , calculate: function (n) {
            return n.itemType === GleamTech.FileUltimate.FileManagerItemType.File ? GleamTech.Util.Path.getExtension(n.name, !0) : ""
        }
    }]
    , checkPermission: function (n) {
        return (this.data.permissions & n) == n
    }
    , checkEitherPermissionName: function (n) {
        for (var i, r, t = 0; t < n.length; t++)
            if (i = n[t], r = GleamTech.FileUltimate.FileManagerPermissionTypes[i], this.checkPermission(r)) return !0;
        return !1
    }
    , hasPermissions: function () {
        return this.data.permissions !== ""
    }
    , combinePermissionsOfRecord: function (n) {
        n.hasPermissions() && (this.hasPermissions() ? this.data.permissions |= n.data.permissions : this.data.permissions = n.data.permissions)
    }
});
Ext.define("GleamTech.FileUltimate.FolderNodeModel", {
    extend: "GleamTech.FileUltimate.FolderItemModel"
    , fields: [{
        name: "expandable"
    }, {
        name: "hash"
    }]
    , constructor: function () {
        this.callParent(arguments);
        this.nameIndexer = {};
        this.on({
            scope: this
            , append: this.onNodeAdded
            , insert: this.onNodeAdded
            , remove: this.onNodeRemove
        })
    }
    , onNodeAdded: function (n, t) {
        n == this && (t.hasOwnProperty("childrenNotLoadedYet") && (t.set("loaded", !1), delete t.childrenNotLoadedYet), this.nameIndexer[Ext.data.SortTypes.asLocaleString(t.data.name)] = t)
    }
    , onNodeRemove: function (n, t) {
        n == this && delete this.nameIndexer[Ext.data.SortTypes.asLocaleString(t.data.name)]
    }
    , set: function () {
        var t = this.callParent(arguments)
            , n = this;
        return n.modified && Ext.Object.each(n.modified, function (t, i) {
            if (t == "name") {
                n.onNameChange(i, n.data.name);
                return !1
            }
            return !0
        }), t
    }
    , onNameChange: function (n, t) {
        var i = this.parentNode;
        i && (delete i.nameIndexer[Ext.data.SortTypes.asLocaleString(n)], i.nameIndexer[Ext.data.SortTypes.asLocaleString(t)] = this)
    }
    , findChildNodeByName: function (n) {
        return this.nameIndexer[Ext.data.SortTypes.asLocaleString(n)]
    }
    , findChildNodeByPath: function (n) {
        for (var r, u = n.split(/[/\\]/g), t = this, i = 0; i < u.length; i++)
            if ((r = u[i], r.length !== 0) && (t = t.findChildNodeByName(r), !t)) return null;
        return t
    }
    , getPathData: function () {
        for (var i = [], u = [], t = this, r, n; t.parentNode;) i.unshift(t.data.name), u.unshift(t.data.hash), t = t.parentNode;
        return r = GleamTech.Util.Path.backSlash, n = {}, n.parts = i, n.rootFolderName = i[0], n.rootFolderHash = u[0], n.relativePath = r + i.slice(1)
            .join(r), n.fullPath = "[" + n.rootFolderName + "]:" + n.relativePath, n
    }
});
Ext.define("GleamTech.FileUltimate.FolderViewModel", {
    extend: "GleamTech.FileUltimate.FolderItemModel"
    , fields: [{
        name: "size"
    }, {
        name: "type"
        , sortType: "asLocaleString"
        , calculate: function (n) {
            return n.systemType ? n.systemType : n.itemType === GleamTech.FileUltimate.FileManagerItemType.File ? n.extension ? GleamTech.Util.Language.getEntry("Label.FileType", n.extension.toUpperCase()) : GleamTech.Util.Language.getEntry("Label.File") : GleamTech.Util.Language.getEntry("Label.FolderType")
        }
    }, {
        name: "systemType"
    }, {
        name: "dateModified"
        , type: "date"
        , dateFormat: "c"
    }]
});
GleamTech.FileUltimate.FileManagerItemType = {
    RootFolder: 1
    , Folder: 2
    , File: 3
};
GleamTech.FileUltimate.FileManagerPermissionTypes = {
    ListSubfolders: 1
    , ListFiles: 2
    , Create: 4
    , Delete: 8
    , Rename: 16
    , Edit: 32
    , Upload: 64
    , Download: 128
    , Compress: 256
    , Extract: 512
    , Cut: 1024
    , Copy: 2048
    , Paste: 4096
    , Preview: 8192
    , Print: 16384
};
GleamTech.FileUltimate.FileManagerChooserType = {
    File: 1
    , Folder: 2
    , FileOrFolder: 3
};
GleamTech.FileUltimate.FileManagerClipboardAction = {
    Copy: 1
    , Cut: 2
};
GleamTech.FileUltimate.FileManagerDropAction = {
    Hidden: 1
    , NotAllowed: 2
    , CopyTo: 3
    , MoveTo: 4
    , UploadTo: 5
};
typeof Crypto != "undefined" && Crypto.util || function () {
    var t = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        , r = window.Crypto = {}
        , i = r.util = {
            rotl: function (n, t) {
                return n << t | n >>> 32 - t
            }
            , rotr: function (n, t) {
                return n << 32 - t | n >>> t
            }
            , endian: function (n) {
                if (n.constructor == Number) return i.rotl(n, 8) & 16711935 | i.rotl(n, 24) & 4278255360;
                for (var t = 0; t < n.length; t++) n[t] = i.endian(n[t]);
                return n
            }
            , randomBytes: function (n) {
                for (var t = []; n > 0; n--) t.push(Math.floor(Math.random() * 256));
                return t
            }
            , bytesToWords: function (n) {
                for (var r = [], t = 0, i = 0; t < n.length; t++ , i += 8) r[i >>> 5] |= n[t] << 24 - i % 32;
                return r
            }
            , wordsToBytes: function (n) {
                for (var i = [], t = 0; t < n.length * 32; t += 8) i.push(n[t >>> 5] >>> 24 - t % 32 & 255);
                return i
            }
            , bytesToHex: function (n) {
                for (var i = [], t = 0; t < n.length; t++) i.push((n[t] >>> 4)
                    .toString(16)), i.push((n[t] & 15)
                        .toString(16));
                return i.join("")
            }
            , hexToBytes: function (n) {
                for (var i = [], t = 0; t < n.length; t += 2) i.push(parseInt(n.substr(t, 2), 16));
                return i
            }
            , bytesToBase64: function (i) {
                var f, r, e, u;
                if (typeof btoa == "function") return btoa(n.bytesToString(i));
                for (f = [], r = 0; r < i.length; r += 3)
                    for (e = i[r] << 16 | i[r + 1] << 8 | i[r + 2], u = 0; u < 4; u++) r * 8 + u * 6 <= i.length * 8 ? f.push(t.charAt(e >>> 6 * (3 - u) & 63)) : f.push("=");
                return f.join("")
            }
            , base64ToBytes: function (i) {
                if (typeof atob == "function") return n.stringToBytes(atob(i));
                i = i.replace(/[^A-Z0-9+\/]/ig, "");
                for (var f = [], u = 0, r = 0; u < i.length; r = ++u % 4) r != 0 && f.push((t.indexOf(i.charAt(u - 1)) & Math.pow(2, -2 * r + 8) - 1) << r * 2 | t.indexOf(i.charAt(u)) >>> 6 - r * 2);
                return f
            }
        }
        , u = r.charenc = {}
        , f = u.UTF8 = {
            stringToBytes: function (t) {
                return n.stringToBytes(unescape(encodeURIComponent(t)))
            }
            , bytesToString: function (t) {
                return decodeURIComponent(escape(n.bytesToString(t)))
            }
        }
        , n = u.Binary = {
            stringToBytes: function (n) {
                for (var i = [], t = 0; t < n.length; t++) i.push(n.charCodeAt(t) & 255);
                return i
            }
            , bytesToString: function (n) {
                for (var i = [], t = 0; t < n.length; t++) i.push(String.fromCharCode(n[t]));
                return i.join("")
            }
        }
}();
(function () {
    var i = Crypto
        , t = i.util
        , r = i.charenc
        , u = r.UTF8
        , f = r.Binary
        , n = i.MD5 = function (i, r) {
            var u = t.wordsToBytes(n._md5(i));
            return r && r.asBytes ? u : r && r.asString ? f.bytesToString(u) : t.bytesToHex(u)
        };
    n._md5 = function (i) {
        var s;
        i.constructor == String && (i = u.stringToBytes(i));
        var h = t.bytesToWords(i)
            , y = i.length * 8
            , r = 1732584193
            , f = -271733879
            , e = -1732584194
            , o = 271733878;
        for (s = 0; s < h.length; s++) h[s] = (h[s] << 8 | h[s] >>> 24) & 16711935 | (h[s] << 24 | h[s] >>> 8) & 4278255360;
        h[y >>> 5] |= 128 << y % 32;
        h[(y + 64 >>> 9 << 4) + 14] = y;
        var c = n._ff
            , l = n._gg
            , a = n._hh
            , v = n._ii;
        for (s = 0; s < h.length; s += 16) {
            var p = r
                , w = f
                , b = e
                , k = o;
            r = c(r, f, e, o, h[s + 0], 7, -680876936);
            o = c(o, r, f, e, h[s + 1], 12, -389564586);
            e = c(e, o, r, f, h[s + 2], 17, 606105819);
            f = c(f, e, o, r, h[s + 3], 22, -1044525330);
            r = c(r, f, e, o, h[s + 4], 7, -176418897);
            o = c(o, r, f, e, h[s + 5], 12, 1200080426);
            e = c(e, o, r, f, h[s + 6], 17, -1473231341);
            f = c(f, e, o, r, h[s + 7], 22, -45705983);
            r = c(r, f, e, o, h[s + 8], 7, 1770035416);
            o = c(o, r, f, e, h[s + 9], 12, -1958414417);
            e = c(e, o, r, f, h[s + 10], 17, -42063);
            f = c(f, e, o, r, h[s + 11], 22, -1990404162);
            r = c(r, f, e, o, h[s + 12], 7, 1804603682);
            o = c(o, r, f, e, h[s + 13], 12, -40341101);
            e = c(e, o, r, f, h[s + 14], 17, -1502002290);
            f = c(f, e, o, r, h[s + 15], 22, 1236535329);
            r = l(r, f, e, o, h[s + 1], 5, -165796510);
            o = l(o, r, f, e, h[s + 6], 9, -1069501632);
            e = l(e, o, r, f, h[s + 11], 14, 643717713);
            f = l(f, e, o, r, h[s + 0], 20, -373897302);
            r = l(r, f, e, o, h[s + 5], 5, -701558691);
            o = l(o, r, f, e, h[s + 10], 9, 38016083);
            e = l(e, o, r, f, h[s + 15], 14, -660478335);
            f = l(f, e, o, r, h[s + 4], 20, -405537848);
            r = l(r, f, e, o, h[s + 9], 5, 568446438);
            o = l(o, r, f, e, h[s + 14], 9, -1019803690);
            e = l(e, o, r, f, h[s + 3], 14, -187363961);
            f = l(f, e, o, r, h[s + 8], 20, 1163531501);
            r = l(r, f, e, o, h[s + 13], 5, -1444681467);
            o = l(o, r, f, e, h[s + 2], 9, -51403784);
            e = l(e, o, r, f, h[s + 7], 14, 1735328473);
            f = l(f, e, o, r, h[s + 12], 20, -1926607734);
            r = a(r, f, e, o, h[s + 5], 4, -378558);
            o = a(o, r, f, e, h[s + 8], 11, -2022574463);
            e = a(e, o, r, f, h[s + 11], 16, 1839030562);
            f = a(f, e, o, r, h[s + 14], 23, -35309556);
            r = a(r, f, e, o, h[s + 1], 4, -1530992060);
            o = a(o, r, f, e, h[s + 4], 11, 1272893353);
            e = a(e, o, r, f, h[s + 7], 16, -155497632);
            f = a(f, e, o, r, h[s + 10], 23, -1094730640);
            r = a(r, f, e, o, h[s + 13], 4, 681279174);
            o = a(o, r, f, e, h[s + 0], 11, -358537222);
            e = a(e, o, r, f, h[s + 3], 16, -722521979);
            f = a(f, e, o, r, h[s + 6], 23, 76029189);
            r = a(r, f, e, o, h[s + 9], 4, -640364487);
            o = a(o, r, f, e, h[s + 12], 11, -421815835);
            e = a(e, o, r, f, h[s + 15], 16, 530742520);
            f = a(f, e, o, r, h[s + 2], 23, -995338651);
            r = v(r, f, e, o, h[s + 0], 6, -198630844);
            o = v(o, r, f, e, h[s + 7], 10, 1126891415);
            e = v(e, o, r, f, h[s + 14], 15, -1416354905);
            f = v(f, e, o, r, h[s + 5], 21, -57434055);
            r = v(r, f, e, o, h[s + 12], 6, 1700485571);
            o = v(o, r, f, e, h[s + 3], 10, -1894986606);
            e = v(e, o, r, f, h[s + 10], 15, -1051523);
            f = v(f, e, o, r, h[s + 1], 21, -2054922799);
            r = v(r, f, e, o, h[s + 8], 6, 1873313359);
            o = v(o, r, f, e, h[s + 15], 10, -30611744);
            e = v(e, o, r, f, h[s + 6], 15, -1560198380);
            f = v(f, e, o, r, h[s + 13], 21, 1309151649);
            r = v(r, f, e, o, h[s + 4], 6, -145523070);
            o = v(o, r, f, e, h[s + 11], 10, -1120210379);
            e = v(e, o, r, f, h[s + 2], 15, 718787259);
            f = v(f, e, o, r, h[s + 9], 21, -343485551);
            r = r + p >>> 0;
            f = f + w >>> 0;
            e = e + b >>> 0;
            o = o + k >>> 0
        }
        return t.endian([r, f, e, o])
    };
    n._ff = function (n, t, i, r, u, f, e) {
        var o = n + (t & i | ~t & r) + (u >>> 0) + e;
        return (o << f | o >>> 32 - f) + t
    };
    n._gg = function (n, t, i, r, u, f, e) {
        var o = n + (t & r | i & ~r) + (u >>> 0) + e;
        return (o << f | o >>> 32 - f) + t
    };
    n._hh = function (n, t, i, r, u, f, e) {
        var o = n + (t ^ i ^ r) + (u >>> 0) + e;
        return (o << f | o >>> 32 - f) + t
    };
    n._ii = function (n, t, i, r, u, f, e) {
        var o = n + (i ^ (t | ~r)) + (u >>> 0) + e;
        return (o << f | o >>> 32 - f) + t
    };
    n._blocksize = 16;
    n._digestsize = 16
})();
var MXI_DEBUG = !1;
(function (n, t) {
    var i = function () {
        var n = {};
        return t.apply(n, arguments), n.moxie
    };
    typeof define == "function" && define.amd ? define("moxie", [], i) : typeof module == "object" && module.exports ? module.exports = i() : n.moxie = i()
})(this || window, function () {
    (function (n, t) {
        "use strict";

        function f(n, t) {
            for (var u, f = [], i = 0; i < n.length; ++i) {
                if (u = r[n[i]] || e(n[i]), !u) throw "module definition dependecy not found: " + n[i];
                f.push(u)
            }
            t.apply(null, f)
        }

        function i(n, i, u) {
            if (typeof n != "string") throw "invalid module definition, module id must be defined and be a string";
            if (i === t) throw "invalid module definition, dependencies must be specified";
            if (u === t) throw "invalid module definition, definition function must be specified";
            f(i, function () {
                r[n] = u.apply(null, arguments)
            })
        }

        function u(n) {
            return !!r[n]
        }

        function e(t) {
            for (var i = n, u = t.split(/[.\/]/), r = 0; r < u.length; ++r) {
                if (!i[u[r]]) return;
                i = i[u[r]]
            }
            return i
        }

        function o(i) {
            for (var f, o = 0; o < i.length; o++) {
                var e = n
                    , s = i[o]
                    , u = s.split(/[.\/]/);
                for (f = 0; f < u.length - 1; ++f) e[u[f]] === t && (e[u[f]] = {}), e = e[u[f]];
                e[u[u.length - 1]] = r[s]
            }
        }
        var r = {};
        i("moxie/core/utils/Basic", [], function () {
            function n(n) {
                var t;
                return n === t ? "undefined" : n === null ? "null" : n.nodeType ? "node" : {}.toString.call(n)
                    .match(/\s([a-z|A-Z]+)/)[1].toLowerCase()
            }

            function u() {
                return t(!1, !1, arguments)
            }

            function f() {
                return t(!0, !1, arguments)
            }

            function e() {
                return t(!1, !0, arguments)
            }

            function o() {
                return t(!0, !0, arguments)
            }

            function s(i) {
                switch (n(i)) {
                    case "array":
                        return t(!1, !0, [[], i]);
                    case "object":
                        return t(!1, !0, [{}, i]);
                    default:
                        return i
                }
            }

            function h(t) {
                switch (n(t)) {
                    case "array":
                        return Array.prototype.slice.call(t);
                    case "object":
                        return u({}, t)
                }
                return t
            }

            function t(u, f, e) {
                var s, o = e[0];
                return i(e, function (e, c) {
                    c > 0 && i(e, function (i, e) {
                        var c = r(n(i), ["array", "object"]) !== -1;
                        if (i === s || u && o[e] === s) return !0;
                        c && f && (i = h(i));
                        n(o[e]) === n(i) && c ? t(u, f, [o[e], i]) : o[e] = i
                    })
                }), o
            }

            function c(n, t) {
                function r() {
                    if (this.constructor = n, MXI_DEBUG) {
                        var t = function (n) {
                            var t = n.toString()
                                .match(/^function\s([^\(\s]+)/);
                            return t ? t[1] : !1
                        };
                        this.ctorName = t(n)
                    }
                }
                for (var i in t) ({})
                    .hasOwnProperty.call(t, i) && (n[i] = t[i]);
                return r.prototype = t.prototype, n.prototype = new r, n.parent = t.prototype, n
            }

            function i(n, t) {
                var i, u, r, f;
                if (n) {
                    try {
                        i = n.length
                    } catch (e) {
                        i = f
                    }
                    if (i === f || typeof i != "number") {
                        for (u in n)
                            if (n.hasOwnProperty(u) && t(n[u], u) === !1) return
                    } else
                        for (r = 0; r < i; r++)
                            if (t(n[r], r) === !1) return
                }
            }

            function l(t) {
                var i;
                if (!t || n(t) !== "object") return !0;
                for (i in t) return !1;
                return !0
            }

            function a(t, i) {
                function r(f) {
                    n(t[f]) === "function" && t[f](function (n) {
                        ++f < u && !n ? r(f) : i(n)
                    })
                }
                var u = t.length;
                n(i) !== "function" && (i = function () { });
                t && t.length || i();
                r(0)
            }

            function v(n, t) {
                var u = 0
                    , f = n.length
                    , r = new Array(f);
                i(n, function (n, i) {
                    n(function (n) {
                        if (n) return t(n);
                        var e = [].slice.call(arguments);
                        e.shift();
                        r[i] = e;
                        u++;
                        u === f && (r.unshift(null), t.apply(this, r))
                    })
                })
            }

            function r(n, t) {
                if (t) {
                    if (Array.prototype.indexOf) return Array.prototype.indexOf.call(t, n);
                    for (var i = 0, r = t.length; i < r; i++)
                        if (t[i] === n) return i
                }
                return -1
            }

            function y(t, i) {
                var u = []
                    , f;
                n(t) !== "array" && (t = [t]);
                n(i) !== "array" && (i = [i]);
                for (f in t) r(t[f], i) === -1 && u.push(t[f]);
                return u.length ? u : !1
            }

            function p(n, t) {
                var u = [];
                return i(n, function (n) {
                    r(n, t) !== -1 && u.push(n)
                }), u.length ? u : null
            }

            function w(n) {
                for (var i = [], t = 0; t < n.length; t++) i[t] = n[t];
                return i
            }

            function k(n) {
                return n ? String.prototype.trim ? String.prototype.trim.call(n) : n.toString()
                    .replace(/^\s*/, "")
                    .replace(/\s*$/, "") : n
            }

            function d(n) {
                if (typeof n != "string") return n;
                var i = {
                    t: 1099511627776
                    , g: 1073741824
                    , m: 1048576
                    , k: 1024
                }
                    , t;
                return n = /^([0-9\.]+)([tmgk]?)$/.exec(n.toLowerCase()
                    .replace(/[^0-9\.tmkg]/g, "")), t = n[2], n = +n[1], i.hasOwnProperty(t) && (n *= i[t]), Math.floor(n)
            }

            function g(n) {
                var t = [].slice.call(arguments, 1);
                return n.replace(/%([a-z])/g, function (n, i) {
                    var r = t.shift();
                    switch (i) {
                        case "s":
                            return r + "";
                        case "d":
                            return parseInt(r, 10);
                        case "f":
                            return parseFloat(r);
                        case "c":
                            return "";
                        default:
                            return r
                    }
                })
            }

            function nt(n, t) {
                var i = this;
                setTimeout(function () {
                    n.call(i)
                }, t || 1)
            }
            var b = function () {
                var n = 0;
                return function (t) {
                    for (var r = (new Date)
                        .getTime()
                        .toString(32), i = 0; i < 5; i++) r += Math.floor(Math.random() * 65535)
                            .toString(32);
                    return (t || "o_") + r + (n++)
                        .toString(32)
                }
            }();
            return {
                guid: b
                , typeOf: n
                , extend: u
                , extendIf: f
                , extendImmutable: e
                , extendImmutableIf: o
                , clone: s
                , inherit: c
                , each: i
                , isEmptyObj: l
                , inSeries: a
                , inParallel: v
                , inArray: r
                , arrayDiff: y
                , arrayIntersect: p
                , toArray: w
                , trim: k
                , sprintf: g
                , parseSizeStr: d
                , delay: nt
            }
        });
        i("moxie/core/utils/Encode", [], function () {
            var t = function (n) {
                return unescape(encodeURIComponent(n))
            }
                , n = function (n) {
                    return decodeURIComponent(escape(n))
                }
                , i = function (t, i) {
                    if (typeof window.atob == "function") return i ? n(window.atob(t)) : window.atob(t);
                    var u = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
                        , f, o, l, a, v, s, h, e, r = 0
                        , p = 0
                        , c = ""
                        , y = [];
                    if (!t) return t;
                    t += "";
                    do a = u.indexOf(t.charAt(r++)), v = u.indexOf(t.charAt(r++)), s = u.indexOf(t.charAt(r++)), h = u.indexOf(t.charAt(r++)), e = a << 18 | v << 12 | s << 6 | h, f = e >> 16 & 255, o = e >> 8 & 255, l = e & 255, y[p++] = s == 64 ? String.fromCharCode(f) : h == 64 ? String.fromCharCode(f, o) : String.fromCharCode(f, o, l); while (r < t.length);
                    return c = y.join(""), i ? n(c) : c
                }
                , r = function (n, i) {
                    var e;
                    if (i && (n = t(n)), typeof window.btoa == "function") return window.btoa(n);
                    var u = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
                        , s, h, c, l, a, v, y, r, f = 0
                        , w = 0
                        , o = ""
                        , p = [];
                    if (!n) return n;
                    do s = n.charCodeAt(f++), h = n.charCodeAt(f++), c = n.charCodeAt(f++), r = s << 16 | h << 8 | c, l = r >> 18 & 63, a = r >> 12 & 63, v = r >> 6 & 63, y = r & 63, p[w++] = u.charAt(l) + u.charAt(a) + u.charAt(v) + u.charAt(y); while (f < n.length);
                    return o = p.join(""), e = n.length % 3, (e ? o.slice(0, e - 3) : o) + "===".slice(e || 3)
                };
            return {
                utf8_encode: t
                , utf8_decode: n
                , atob: i
                , btoa: r
            }
        });
        i("moxie/core/utils/Env", ["moxie/core/utils/Basic"], function (n) {
            function f(n, t, i) {
                var r = 0
                    , f = 0
                    , u = 0
                    , s = {
                        dev: -6
                        , alpha: -5
                        , a: -5
                        , beta: -4
                        , b: -4
                        , RC: -3
                        , rc: -3
                        , "#": -2
                        , p: 1
                        , pl: 1
                    }
                    , e = function (n) {
                        return n = ("" + n)
                            .replace(/[_\-+]/g, "."), n = n.replace(/([^.\d]+)/g, ".$1.")
                                .replace(/\.{2,}/g, "."), n.length ? n.split(".") : [-8]
                    }
                    , o = function (n) {
                        return n ? isNaN(n) ? s[n] || -7 : parseInt(n, 10) : 0
                    };
                for (n = e(n), t = e(t), f = Math.max(n.length, t.length), r = 0; r < f; r++)
                    if (n[r] != t[r])
                        if (n[r] = o(n[r]), t[r] = o(t[r]), n[r] < t[r]) {
                            u = -1;
                            break
                        } else if (n[r] > t[r]) {
                            u = 1;
                            break
                        }
                if (!i) return u;
                switch (i) {
                    case ">":
                    case "gt":
                        return u > 0;
                    case ">=":
                    case "ge":
                        return u >= 0;
                    case "<=":
                    case "le":
                        return u <= 0;
                    case "==":
                    case "=":
                    case "eq":
                        return u === 0;
                    case "<>":
                    case "!=":
                    case "ne":
                        return u !== 0;
                    case "":
                    case "<":
                    case "lt":
                        return u < 0;
                    default:
                        return null
                }
            }
            var u = function (n) {
                var c = ""
                    , o = "?"
                    , s = "function"
                    , l = "undefined"
                    , u = "object"
                    , t = "name"
                    , i = "version"
                    , h = {
                        has: function (n, t) {
                            return t.toLowerCase()
                                .indexOf(n.toLowerCase()) !== -1
                        }
                        , lowerize: function (n) {
                            return n.toLowerCase()
                        }
                    }
                    , r = {
                        rgx: function () {
                            for (var r, h, y, f, t, c, i, v, o, e = 0, a = arguments; e < a.length; e += 2) {
                                if (v = a[e], o = a[e + 1], typeof r === l) {
                                    r = {};
                                    for (f in o) t = o[f], typeof t === u ? r[t[0]] = n : r[t] = n
                                }
                                for (h = y = 0; h < v.length; h++)
                                    if (c = v[h].exec(this.getUA()), !!c) {
                                        for (f = 0; f < o.length; f++) i = c[++y], t = o[f], typeof t === u && t.length > 0 ? t.length == 2 ? r[t[0]] = typeof t[1] == s ? t[1].call(this, i) : t[1] : t.length == 3 ? r[t[0]] = typeof t[1] !== s || t[1].exec && t[1].test ? i ? i.replace(t[1], t[2]) : n : i ? t[1].call(this, i, t[2]) : n : t.length == 4 && (r[t[0]] = i ? t[3].call(this, i.replace(t[1], t[2])) : n) : r[t] = i ? i : n;
                                        break
                                    } if (!!c) break
                            }
                            return r
                        }
                        , str: function (t, i) {
                            var r, f;
                            for (r in i)
                                if (typeof i[r] === u && i[r].length > 0) {
                                    for (f = 0; f < i[r].length; f++)
                                        if (h.has(i[r][f], t)) return r === o ? n : r
                                } else if (h.has(i[r], t)) return r === o ? n : r;
                            return t
                        }
                    }
                    , f = {
                        browser: {
                            oldsafari: {
                                major: {
                                    "1": ["/8", "/1", "/3"]
                                    , "2": "/4"
                                    , "?": "/"
                                }
                                , version: {
                                    "1.0": "/8"
                                    , "1.2": "/1"
                                    , "1.3": "/3"
                                    , "2.0": "/412"
                                    , "2.0.2": "/416"
                                    , "2.0.3": "/417"
                                    , "2.0.4": "/419"
                                    , "?": "/"
                                }
                            }
                        }
                        , device: {
                            sprint: {
                                model: {
                                    "Evo Shift 4G": "7373KT"
                                }
                                , vendor: {
                                    HTC: "APA"
                                    , Sprint: "Sprint"
                                }
                            }
                        }
                        , os: {
                            windows: {
                                version: {
                                    ME: "4.90"
                                    , "NT 3.11": "NT3.51"
                                    , "NT 4.0": "NT4.0"
                                    , "2000": "NT 5.0"
                                    , XP: ["NT 5.1", "NT 5.2"]
                                    , Vista: "NT 6.0"
                                    , "7": "NT 6.1"
                                    , "8": "NT 6.2"
                                    , "8.1": "NT 6.3"
                                    , RT: "ARM"
                                }
                            }
                        }
                    }
                    , e = {
                        browser: [[/(opera\smini)\/([\w\.-]+)/i, /(opera\s[mobiletab]+).+version\/([\w\.-]+)/i, /(opera).+version\/([\w\.]+)/i, /(opera)[\/\s]+([\w\.]+)/i], [t, i], [/\s(opr)\/([\w\.]+)/i], [[t, "Opera"], i], [/(kindle)\/([\w\.]+)/i, /(lunascape|maxthon|netfront|jasmine|blazer)[\/\s]?([\w\.]+)*/i, /(avant\s|iemobile|slim|baidu)(?:browser)?[\/\s]?([\w\.]*)/i, /(?:ms|\()(ie)\s([\w\.]+)/i, /(rekonq)\/([\w\.]+)*/i, /(chromium|flock|rockmelt|midori|epiphany|silk|skyfire|ovibrowser|bolt|iron|vivaldi)\/([\w\.-]+)/i], [t, i], [/(trident).+rv[:\s]([\w\.]+).+like\sgecko/i], [[t, "IE"], i], [/(edge)\/((\d+)?[\w\.]+)/i], [t, i], [/(yabrowser)\/([\w\.]+)/i], [[t, "Yandex"], i], [/(comodo_dragon)\/([\w\.]+)/i], [[t, /_/g, " "], i], [/(chrome|omniweb|arora|[tizenoka]{5}\s?browser)\/v?([\w\.]+)/i, /(uc\s?browser|qqbrowser)[\/\s]?([\w\.]+)/i], [t, i], [/(dolfin)\/([\w\.]+)/i], [[t, "Dolphin"], i], [/((?:android.+)crmo|crios)\/([\w\.]+)/i], [[t, "Chrome"], i], [/XiaoMi\/MiuiBrowser\/([\w\.]+)/i], [i, [t, "MIUI Browser"]], [/android.+version\/([\w\.]+)\s+(?:mobile\s?safari|safari)/i], [i, [t, "Android Browser"]], [/FBAV\/([\w\.]+);/i], [i, [t, "Facebook"]], [/version\/([\w\.]+).+?mobile\/\w+\s(safari)/i], [i, [t, "Mobile Safari"]], [/version\/([\w\.]+).+?(mobile\s?safari|safari)/i], [i, t], [/webkit.+?(mobile\s?safari|safari)(\/[\w\.]+)/i], [t, [i, r.str, f.browser.oldsafari.version]], [/(konqueror)\/([\w\.]+)/i, /(webkit|khtml)\/([\w\.]+)/i], [t, i], [/(navigator|netscape)\/([\w\.-]+)/i], [[t, "Netscape"], i], [/(swiftfox)/i, /(icedragon|iceweasel|camino|chimera|fennec|maemo\sbrowser|minimo|conkeror)[\/\s]?([\w\.\+]+)/i, /(firefox|seamonkey|k-meleon|icecat|iceape|firebird|phoenix)\/([\w\.-]+)/i, /(mozilla)\/([\w\.]+).+rv\:.+gecko\/\d+/i, /(polaris|lynx|dillo|icab|doris|amaya|w3m|netsurf)[\/\s]?([\w\.]+)/i, /(links)\s\(([\w\.]+)/i, /(gobrowser)\/?([\w\.]+)*/i, /(ice\s?browser)\/v?([\w\._]+)/i, /(mosaic)[\/\s]([\w\.]+)/i], [t, i]]
                        , engine: [[/windows.+\sedge\/([\w\.]+)/i], [i, [t, "EdgeHTML"]], [/(presto)\/([\w\.]+)/i, /(webkit|trident|netfront|netsurf|amaya|lynx|w3m)\/([\w\.]+)/i, /(khtml|tasman|links)[\/\s]\(?([\w\.]+)/i, /(icab)[\/\s]([23]\.[\d\.]+)/i], [t, i], [/rv\:([\w\.]+).*(gecko)/i], [i, t]]
                        , os: [[/microsoft\s(windows)\s(vista|xp)/i], [t, i], [/(windows)\snt\s6\.2;\s(arm)/i, /(windows\sphone(?:\sos)*|windows\smobile|windows)[\s\/]?([ntce\d\.\s]+\w)/i], [t, [i, r.str, f.os.windows.version]], [/(win(?=3|9|n)|win\s9x\s)([nt\d\.]+)/i], [[t, "Windows"], [i, r.str, f.os.windows.version]], [/\((bb)(10);/i], [[t, "BlackBerry"], i], [/(blackberry)\w*\/?([\w\.]+)*/i, /(tizen)[\/\s]([\w\.]+)/i, /(android|webos|palm\os|qnx|bada|rim\stablet\sos|meego|contiki)[\/\s-]?([\w\.]+)*/i, /linux;.+(sailfish);/i], [t, i], [/(symbian\s?os|symbos|s60(?=;))[\/\s-]?([\w\.]+)*/i], [[t, "Symbian"], i], [/\((series40);/i], [t], [/mozilla.+\(mobile;.+gecko.+firefox/i], [[t, "Firefox OS"], i], [/(nintendo|playstation)\s([wids3portablevu]+)/i, /(mint)[\/\s\(]?(\w+)*/i, /(mageia|vectorlinux)[;\s]/i, /(joli|[kxln]?ubuntu|debian|[open]*suse|gentoo|arch|slackware|fedora|mandriva|centos|pclinuxos|redhat|zenwalk|linpus)[\/\s-]?([\w\.-]+)*/i, /(hurd|linux)\s?([\w\.]+)*/i, /(gnu)\s?([\w\.]+)*/i], [t, i], [/(cros)\s[\w]+\s([\w\.]+\w)/i], [[t, "Chromium OS"], i], [/(sunos)\s?([\w\.]+\d)*/i], [[t, "Solaris"], i], [/\s([frentopc-]{0,4}bsd|dragonfly)\s?([\w\.]+)*/i], [t, i], [/(ip[honead]+)(?:.*os\s*([\w]+)*\slike\smac|;\sopera)/i], [[t, "iOS"], [i, /_/g, "."]], [/(mac\sos\sx)\s?([\w\s\.]+\w)*/i, /(macintosh|mac(?=_powerpc)\s)/i], [[t, "Mac OS"], [i, /_/g, "."]], [/((?:open)?solaris)[\/\s-]?([\w\.]+)*/i, /(haiku)\s(\w+)/i, /(aix)\s((\d)(?=\.|\)|\s)[\w\.]*)*/i, /(plan\s9|minix|beos|os\/2|amigaos|morphos|risc\sos|openvms)/i, /(unix)\s?([\w\.]+)*/i], [t, i]]
                    };
                return function (n) {
                    var t = n || (window && window.navigator && window.navigator.userAgent ? window.navigator.userAgent : c);
                    this.getBrowser = function () {
                        return r.rgx.apply(this, e.browser)
                    };
                    this.getEngine = function () {
                        return r.rgx.apply(this, e.engine)
                    };
                    this.getOS = function () {
                        return r.rgx.apply(this, e.os)
                    };
                    this.getResult = function () {
                        return {
                            ua: this.getUA()
                            , browser: this.getBrowser()
                            , engine: this.getEngine()
                            , os: this.getOS()
                        }
                    };
                    this.getUA = function () {
                        return t
                    };
                    this.setUA = function (n) {
                        return t = n, this
                    };
                    this.setUA(t)
                }
            }()
                , e = function () {
                    var r = {
                        access_global_ns: function () {
                            return !!window.moxie
                        }
                        , define_property: function () {
                            return !1
                        }()
                        , create_canvas: function () {
                            var n = document.createElement("canvas")
                                , t = !!(n.getContext && n.getContext("2d"));
                            return r.create_canvas = t, t
                        }
                        , return_response_type: function (t) {
                            try {
                                if (n.inArray(t, ["", "text", "document"]) !== -1) return !0;
                                if (window.XMLHttpRequest) {
                                    var i = new XMLHttpRequest;
                                    if (i.open("get", "/"), "responseType" in i) return (i.responseType = t, i.responseType !== t) ? !1 : !0
                                }
                            } catch (r) { }
                            return !1
                        }
                        , use_blob_uri: function () {
                            var n = window.URL;
                            return r.use_blob_uri = n && "createObjectURL" in n && "revokeObjectURL" in n && (i.browser !== "IE" || i.verComp(i.version, "11.0.46", ">=")), r.use_blob_uri
                        }
                        , use_data_uri: function () {
                            var n = new Image;
                            return n.onload = function () {
                                r.use_data_uri = n.width === 1 && n.height === 1
                            }, setTimeout(function () {
                                n.src = "data:image/gif;base64,R0lGODlhAQABAIAAAP8AAAAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw=="
                            }, 1), !1
                        }()
                        , use_data_uri_over32kb: function () {
                            return r.use_data_uri && (i.browser !== "IE" || i.version >= 9)
                        }
                        , use_data_uri_of: function (n) {
                            return r.use_data_uri && n < 33e3 || r.use_data_uri_over32kb()
                        }
                        , use_fileinput: function () {
                            if (navigator.userAgent.match(/(Android (1.0|1.1|1.5|1.6|2.0|2.1))|(Windows Phone (OS 7|8.0))|(XBLWP)|(ZuneWP)|(w(eb)?OSBrowser)|(webOS)|(Kindle\/(1.0|2.0|2.5|3.0))/)) return !1;
                            var n = document.createElement("input");
                            return n.setAttribute("type", "file"), r.use_fileinput = !n.disabled
                        }
                        , use_webgl: function () {
                            var i = document.createElement("canvas")
                                , n = null
                                , u;
                            try {
                                n = i.getContext("webgl") || i.getContext("experimental-webgl")
                            } catch (f) { }
                            return n || (n = null), u = !!n, r.use_webgl = u, i = t, u
                        }
                    };
                    return function (t) {
                        var i = [].slice.call(arguments);
                        return i.shift(), n.typeOf(r[t]) === "function" ? r[t].apply(this, i) : !!r[t]
                    }
                }()
                , r = (new u)
                    .getResult()
                , i = {
                    can: e
                    , uaParser: u
                    , browser: r.browser.name
                    , version: r.browser.version
                    , os: r.os.name
                    , osVersion: r.os.version
                    , verComp: f
                    , swf_url: "../flash/Moxie.swf"
                    , xap_url: "../silverlight/Moxie.xap"
                    , global_event_dispatcher: "moxie.core.EventTarget.instance.dispatchEvent"
                };
            return i.OS = i.os, MXI_DEBUG && (i.debug = {
                runtime: !0
                , events: !1
            }, i.log = function () {
                function r(n) {
                    t.appendChild(document.createTextNode(n + "\n"))
                }
                var t, i;
                if (window && window.console && window.console.log && window.console.log.apply) window.console.log.apply(window.console, arguments);
                else if (document) {
                    if (t = document.getElementById("moxie-console"), t || (t = document.createElement("pre"), t.id = "moxie-console", document.body.appendChild(t)), i = arguments[0], n.typeOf(i) === "string") i = n.sprintf.apply(this, arguments);
                    else if (n.inArray(n.typeOf(i), ["object", "array"]) !== -1) {
                        r(i);
                        return
                    }
                    t.appendChild(document.createTextNode(i + "\n"))
                }
            }), i
        });
        i("moxie/core/Exceptions", ["moxie/core/utils/Basic"], function (n) {
            function t(n, t) {
                for (var i in n)
                    if (n[i] === t) return i;
                return null
            }
            return {
                RuntimeError: function () {
                    function i(n, i) {
                        this.code = n;
                        this.name = t(r, n);
                        this.message = this.name + (i || ": RuntimeError " + this.code)
                    }
                    var r = {
                        NOT_INIT_ERR: 1
                        , EXCEPTION_ERR: 3
                        , NOT_SUPPORTED_ERR: 9
                        , JS_ERR: 4
                    };
                    return n.extend(i, r), i.prototype = Error.prototype, i
                }()
                , OperationNotAllowedException: function () {
                    function t(n) {
                        this.code = n;
                        this.name = "OperationNotAllowedException"
                    }
                    return n.extend(t, {
                        NOT_ALLOWED_ERR: 1
                    }), t.prototype = Error.prototype, t
                }()
                , ImageError: function () {
                    function i(n) {
                        this.code = n;
                        this.name = t(r, n);
                        this.message = this.name + ": ImageError " + this.code
                    }
                    var r = {
                        WRONG_FORMAT: 1
                        , MAX_RESOLUTION_ERR: 2
                        , INVALID_META_ERR: 3
                    };
                    return n.extend(i, r), i.prototype = Error.prototype, i
                }()
                , FileException: function () {
                    function i(n) {
                        this.code = n;
                        this.name = t(r, n);
                        this.message = this.name + ": FileException " + this.code
                    }
                    var r = {
                        NOT_FOUND_ERR: 1
                        , SECURITY_ERR: 2
                        , ABORT_ERR: 3
                        , NOT_READABLE_ERR: 4
                        , ENCODING_ERR: 5
                        , NO_MODIFICATION_ALLOWED_ERR: 6
                        , INVALID_STATE_ERR: 7
                        , SYNTAX_ERR: 8
                    };
                    return n.extend(i, r), i.prototype = Error.prototype, i
                }()
                , DOMException: function () {
                    function i(n) {
                        this.code = n;
                        this.name = t(r, n);
                        this.message = this.name + ": DOMException " + this.code
                    }
                    var r = {
                        INDEX_SIZE_ERR: 1
                        , DOMSTRING_SIZE_ERR: 2
                        , HIERARCHY_REQUEST_ERR: 3
                        , WRONG_DOCUMENT_ERR: 4
                        , INVALID_CHARACTER_ERR: 5
                        , NO_DATA_ALLOWED_ERR: 6
                        , NO_MODIFICATION_ALLOWED_ERR: 7
                        , NOT_FOUND_ERR: 8
                        , NOT_SUPPORTED_ERR: 9
                        , INUSE_ATTRIBUTE_ERR: 10
                        , INVALID_STATE_ERR: 11
                        , SYNTAX_ERR: 12
                        , INVALID_MODIFICATION_ERR: 13
                        , NAMESPACE_ERR: 14
                        , INVALID_ACCESS_ERR: 15
                        , VALIDATION_ERR: 16
                        , TYPE_MISMATCH_ERR: 17
                        , SECURITY_ERR: 18
                        , NETWORK_ERR: 19
                        , ABORT_ERR: 20
                        , URL_MISMATCH_ERR: 21
                        , QUOTA_EXCEEDED_ERR: 22
                        , TIMEOUT_ERR: 23
                        , INVALID_NODE_TYPE_ERR: 24
                        , DATA_CLONE_ERR: 25
                    };
                    return n.extend(i, r), i.prototype = Error.prototype, i
                }()
                , EventException: function () {
                    function t(n) {
                        this.code = n;
                        this.name = "EventException"
                    }
                    return n.extend(t, {
                        UNSPECIFIED_EVENT_TYPE_ERR: 0
                    }), t.prototype = Error.prototype, t
                }()
            }
        });
        i("moxie/core/utils/Dom", ["moxie/core/utils/Env"], function (n) {
            var i = function (n) {
                return typeof n != "string" ? n : document.getElementById(n)
            }
                , t = function (n, t) {
                    if (!n.className) return !1;
                    var i = new RegExp("(^|\\s+)" + t + "(\\s+|$)");
                    return i.test(n.className)
                }
                , r = function (n, i) {
                    t(n, i) || (n.className = n.className ? n.className.replace(/\s+$/, "") + " " + i : i)
                }
                , u = function (n, t) {
                    if (n.className) {
                        var i = new RegExp("(^|\\s+)" + t + "(\\s+|$)");
                        n.className = n.className.replace(i, function (n, t, i) {
                            return t === " " && i === " " ? " " : ""
                        })
                    }
                }
                , f = function (n, t) {
                    return n.currentStyle ? n.currentStyle[t] : window.getComputedStyle ? window.getComputedStyle(n, null)[t] : void 0
                }
                , e = function (t, i) {
                    function h(n) {
                        var t, i, r = 0
                            , f = 0;
                        return n && (i = n.getBoundingClientRect(), t = u.compatMode === "CSS1Compat" ? u.documentElement : u.body, r = i.left + t.scrollLeft, f = i.top + t.scrollTop), {
                            x: r
                            , y: f
                        }
                    }
                    var f = 0
                        , e = 0
                        , r, u = document
                        , o, s;
                    if (t = t, i = i || u.body, t && t.getBoundingClientRect && n.browser === "IE" && (!u.documentMode || u.documentMode < 8)) return o = h(t), s = h(i), {
                        x: o.x - s.x
                        , y: o.y - s.y
                    };
                    for (r = t; r && r != i && r.nodeType;) f += r.offsetLeft || 0, e += r.offsetTop || 0, r = r.offsetParent;
                    for (r = t.parentNode; r && r != i && r.nodeType;) f -= r.scrollLeft || 0, e -= r.scrollTop || 0, r = r.parentNode;
                    return {
                        x: f
                        , y: e
                    }
                }
                , o = function (n) {
                    return {
                        w: n.offsetWidth || n.clientWidth
                        , h: n.offsetHeight || n.clientHeight
                    }
                };
            return {
                get: i
                , hasClass: t
                , addClass: r
                , removeClass: u
                , getStyle: f
                , getPos: e
                , getSize: o
            }
        });
        i("moxie/core/EventTarget", ["moxie/core/utils/Env", "moxie/core/Exceptions", "moxie/core/utils/Basic"], function (n, t, i) {
            function u() {
                this.uid = i.guid()
            }
            var r = {};
            return i.extend(u.prototype, {
                init: function () {
                    this.uid || (this.uid = i.guid("uid_"))
                }
                , addEventListener: function (n, t, u, f) {
                    var o = this
                        , e;
                    if (this.hasOwnProperty("uid") || (this.uid = i.guid("uid_")), n = i.trim(n), /\s/.test(n)) {
                        i.each(n.split(/\s+/), function (n) {
                            o.addEventListener(n, t, u, f)
                        });
                        return
                    }
                    n = n.toLowerCase();
                    u = parseInt(u, 10) || 0;
                    e = r[this.uid] && r[this.uid][n] || [];
                    e.push({
                        fn: t
                        , priority: u
                        , scope: f || this
                    });
                    r[this.uid] || (r[this.uid] = {});
                    r[this.uid][n] = e
                }
                , hasEventListener: function (n) {
                    var t;
                    return n ? (n = n.toLowerCase(), t = r[this.uid] && r[this.uid][n]) : t = r[this.uid], t ? t : !1
                }
                , removeEventListener: function (n, t) {
                    var e = this
                        , u, f;
                    if (n = n.toLowerCase(), /\s/.test(n)) {
                        i.each(n.split(/\s+/), function (n) {
                            e.removeEventListener(n, t)
                        });
                        return
                    }
                    if (u = r[this.uid] && r[this.uid][n], u) {
                        if (t) {
                            for (f = u.length - 1; f >= 0; f--)
                                if (u[f].fn === t) {
                                    u.splice(f, 1);
                                    break
                                }
                        } else u = [];
                        u.length || (delete r[this.uid][n], i.isEmptyObj(r[this.uid]) && delete r[this.uid])
                    }
                }
                , removeAllEventListeners: function () {
                    r[this.uid] && delete r[this.uid]
                }
                , dispatchEvent: function (u) {
                    var s, c, o, f, e = {}
                        , l = !0
                        , a, h;
                    if (i.typeOf(u) !== "string")
                        if (f = u, i.typeOf(f.type) === "string") u = f.type, f.total !== a && f.loaded !== a && (e.total = f.total, e.loaded = f.loaded), e.async = f.async || !1;
                        else throw new t.EventException(t.EventException.UNSPECIFIED_EVENT_TYPE_ERR);
                    return u.indexOf("::") !== -1 ? function (n) {
                        s = n[0];
                        u = n[1]
                    }(u.split("::")) : s = this.uid, u = u.toLowerCase(), c = r[s] && r[s][u], c && (c.sort(function (n, t) {
                        return t.priority - n.priority
                    }), o = [].slice.call(arguments), o.shift(), e.type = u, o.unshift(e), MXI_DEBUG && n.debug.events && n.log("%cEvent '%s' fired on %s", "color: #999;", e.type, (this.ctorName ? this.ctorName + "::" : "") + s), h = [], i.each(c, function (n) {
                        o[0].target = n.scope;
                        e.async ? h.push(function (t) {
                            setTimeout(function () {
                                t(n.fn.apply(n.scope, o) === !1)
                            }, 1)
                        }) : h.push(function (t) {
                            t(n.fn.apply(n.scope, o) === !1)
                        })
                    }), h.length && i.inSeries(h, function (n) {
                        l = !n
                    })), l
                }
                , bindOnce: function (n, t, i, r) {
                    var u = this;
                    u.bind.call(this, n, function f() {
                        return u.unbind(n, f), t.apply(this, arguments)
                    }, i, r)
                }
                , bind: function () {
                    this.addEventListener.apply(this, arguments)
                }
                , unbind: function () {
                    this.removeEventListener.apply(this, arguments)
                }
                , unbindAll: function () {
                    this.removeAllEventListeners.apply(this, arguments)
                }
                , trigger: function () {
                    return this.dispatchEvent.apply(this, arguments)
                }
                , handleEventProps: function (n) {
                    var t = this;
                    this.bind(n.join(" "), function (n) {
                        var t = "on" + n.type.toLowerCase();
                        i.typeOf(this[t]) === "function" && this[t].apply(this, arguments)
                    });
                    i.each(n, function (n) {
                        n = "on" + n.toLowerCase(n);
                        i.typeOf(t[n]) === "undefined" && (t[n] = null)
                    })
                }
            }), u.instance = new u, u
        });
        i("moxie/runtime/Runtime", ["moxie/core/utils/Env", "moxie/core/utils/Basic", "moxie/core/utils/Dom", "moxie/core/EventTarget"], function (n, t, i, r) {
            function u(r, e, o, s, h) {
                var c = this
                    , l, a = t.guid(e + "_")
                    , v = h || "browser";
                r = r || {};
                f[a] = this;
                o = t.extend({
                    access_binary: !1
                    , access_image_binary: !1
                    , display_media: !1
                    , do_cors: !1
                    , drag_and_drop: !1
                    , filter_by_extension: !0
                    , resize_image: !1
                    , report_upload_progress: !1
                    , return_response_headers: !1
                    , return_response_type: !1
                    , return_status_code: !0
                    , send_custom_headers: !1
                    , select_file: !1
                    , select_folder: !1
                    , select_multiple: !0
                    , send_binary_string: !1
                    , send_browser_cookies: !0
                    , send_multipart: !0
                    , slice_blob: !1
                    , stream_upload: !1
                    , summon_file_dialog: !1
                    , upload_filesize: !0
                    , use_http_method: !0
                }, o);
                r.preferred_caps && (v = u.getMode(s, r.preferred_caps, v));
                MXI_DEBUG && n.debug.runtime && n.log("\tdefault mode: %s", v);
                l = function () {
                    var n = {};
                    return {
                        exec: function (t, i, r, u) {
                            if (l[i] && (n[t] || (n[t] = {
                                context: this
                                , instance: new l[i]
                            }), n[t].instance[r])) return n[t].instance[r].apply(this, u)
                        }
                        , removeInstance: function (t) {
                            delete n[t]
                        }
                        , removeAllInstances: function () {
                            var i = this;
                            t.each(n, function (n, r) {
                                t.typeOf(n.instance.destroy) === "function" && n.instance.destroy.call(n.context);
                                i.removeInstance(r)
                            })
                        }
                    }
                }();
                t.extend(this, {
                    initialized: !1
                    , uid: a
                    , type: e
                    , mode: u.getMode(s, r.required_caps, v)
                    , shimid: a + "_container"
                    , clients: 0
                    , options: r
                    , can: function (n, i) {
                        var r = arguments[2] || o
                            , f;
                        if (t.typeOf(n) === "string" && t.typeOf(i) === "undefined" && (n = u.parseCaps(n)), t.typeOf(n) === "object") {
                            for (f in n)
                                if (!this.can(f, n[f], r)) return !1;
                            return !0
                        }
                        return t.typeOf(r[n]) === "function" ? r[n].call(this, i) : i === r[n]
                    }
                    , getShimContainer: function () {
                        var r, n = i.get(this.shimid);
                        return n || (r = i.get(this.options.container) || document.body, n = document.createElement("div"), n.id = this.shimid, n.className = "moxie-shim moxie-shim-" + this.type, t.extend(n.style, {
                            position: "absolute"
                            , top: "0px"
                            , left: "0px"
                            , width: "1px"
                            , height: "1px"
                            , overflow: "hidden"
                        }), r.appendChild(n), r = null), n
                    }
                    , getShim: function () {
                        return l
                    }
                    , shimExec: function (n, t) {
                        var i = [].slice.call(arguments, 2);
                        return c.getShim()
                            .exec.call(this, this.uid, n, t, i)
                    }
                    , exec: function (n, t) {
                        var i = [].slice.call(arguments, 2);
                        return c[n] && c[n][t] ? c[n][t].apply(this, i) : c.shimExec.apply(this, arguments)
                    }
                    , destroy: function () {
                        if (c) {
                            var n = i.get(this.shimid);
                            n && n.parentNode.removeChild(n);
                            l && l.removeAllInstances();
                            this.unbindAll();
                            delete f[this.uid];
                            this.uid = null;
                            a = c = l = n = null
                        }
                    }
                });
                this.mode && r.required_caps && !this.can(r.required_caps) && (this.mode = !1)
            }
            var e = {}
                , f = {};
            return u.order = "html5,flash,silverlight,html4", u.getRuntime = function (n) {
                return f[n] ? f[n] : !1
            }, u.addConstructor = function (n, t) {
                t.prototype = r.instance;
                e[n] = t
            }, u.getConstructor = function (n) {
                return e[n] || null
            }, u.getInfo = function (n) {
                var t = u.getRuntime(n);
                return t ? {
                    uid: t.uid
                    , type: t.type
                    , mode: t.mode
                    , can: function () {
                        return t.can.apply(t, arguments)
                    }
                } : null
            }, u.parseCaps = function (n) {
                var i = {};
                return t.typeOf(n) !== "string" ? n || {} : (t.each(n.split(","), function (n) {
                    i[n] = !0
                }), i)
            }, u.can = function (n, t) {
                var i, r = u.getConstructor(n)
                    , f;
                return r ? (i = new r({
                    required_caps: t
                }), f = i.mode, i.destroy(), !!f) : !1
            }, u.thatCan = function (n, t) {
                var i = (t || u.order)
                    .split(/\s*,\s*/);
                for (var r in i)
                    if (u.can(i[r], n)) return i[r];
                return null
            }, u.getMode = function (i, r, u) {
                var f = null;
                if (t.typeOf(u) === "undefined" && (u = "browser"), r && !t.isEmptyObj(i)) {
                    if (t.each(r, function (r, u) {
                        if (i.hasOwnProperty(u)) {
                            var e = i[u](r);
                            if (typeof e == "string" && (e = [e]), f) {
                                if (!(f = t.arrayIntersect(f, e))) return MXI_DEBUG && n.debug.runtime && n.log("\t\t%s: %s (conflicting mode requested: %s)", u, r, e), f = !1
                            } else f = e
                        }
                        MXI_DEBUG && n.debug.runtime && n.log("\t\t%s: %s (compatible modes: %s)", u, r, f)
                    }), f) return t.inArray(u, f) !== -1 ? u : f[0];
                    if (f === !1) return !1
                }
                return u
            }, u.getGlobalEventTarget = function () {
                if (/^moxie\./.test(n.global_event_dispatcher) && !n.can("access_global_ns")) {
                    var i = t.guid("moxie_event_target_");
                    window[i] = function (n, t) {
                        r.instance.dispatchEvent(n, t)
                    };
                    n.global_event_dispatcher = i
                }
                return n.global_event_dispatcher
            }, u.capTrue = function () {
                return !0
            }, u.capFalse = function () {
                return !1
            }, u.capTest = function (n) {
                return function () {
                    return !!n
                }
            }, u
        });
        i("moxie/runtime/RuntimeClient", ["moxie/core/utils/Env", "moxie/core/Exceptions", "moxie/core/utils/Basic", "moxie/runtime/Runtime"], function (n, t, i, r) {
            return function () {
                var u;
                i.extend(this, {
                    connectRuntime: function (f) {
                        function s(i) {
                            var o, h;
                            if (!i.length) {
                                e.trigger("RuntimeError", new t.RuntimeError(t.RuntimeError.NOT_INIT_ERR));
                                u = null;
                                return
                            }
                            if (o = i.shift()
                                .toLowerCase(), h = r.getConstructor(o), !h) {
                                MXI_DEBUG && n.debug.runtime && n.log("Constructor for '%s' runtime is not available.", o);
                                s(i);
                                return
                            }
                            if (MXI_DEBUG && n.debug.runtime && (n.log("Trying runtime: %s", o), n.log(f)), u = new h(f), u.bind("Init", function () {
                                u.initialized = !0;
                                MXI_DEBUG && n.debug.runtime && n.log("Runtime '%s' initialized", u.type);
                                setTimeout(function () {
                                    u.clients++;
                                    e.ruid = u.uid;
                                    e.trigger("RuntimeInit", u)
                                }, 1)
                            }), u.bind("Error", function () {
                                MXI_DEBUG && n.debug.runtime && n.log("Runtime '%s' failed to initialize", u.type);
                                u.destroy();
                                s(i)
                            }), u.bind("Exception", function (i, r) {
                                var u = r.name + "(#" + r.code + ")" + (r.message ? ", from: " + r.message : "");
                                MXI_DEBUG && n.debug.runtime && n.log("Runtime '%s' has thrown an exception: %s", this.type, u);
                                e.trigger("RuntimeError", new t.RuntimeError(t.RuntimeError.EXCEPTION_ERR, u))
                            }), MXI_DEBUG && n.debug.runtime && n.log("\tselected mode: %s", u.mode), !u.mode) {
                                u.trigger("Error");
                                return
                            }
                            u.init()
                        }
                        var e = this
                            , o;
                        if (i.typeOf(f) === "string" ? o = f : i.typeOf(f.ruid) === "string" && (o = f.ruid), o) {
                            if (u = r.getRuntime(o), u) return e.ruid = o, u.clients++ , u;
                            throw new t.RuntimeError(t.RuntimeError.NOT_INIT_ERR);
                        }
                        s((f.runtime_order || r.order)
                            .split(/\s*,\s*/))
                    }
                    , disconnectRuntime: function () {
                        u && --u.clients <= 0 && u.destroy();
                        u = null
                    }
                    , getRuntime: function () {
                        return u && u.uid ? u : u = null
                    }
                    , exec: function () {
                        return u ? u.exec.apply(this, arguments) : null
                    }
                    , can: function (n) {
                        return u ? u.can(n) : !1
                    }
                })
            }
        });
        i("moxie/file/Blob", ["moxie/core/utils/Basic", "moxie/core/utils/Encode", "moxie/runtime/RuntimeClient"], function (n, t, i) {
            function u(f, e) {
                function o(t, i, f) {
                    var e, o = r[this.uid];
                    return n.typeOf(o) !== "string" || !o.length ? null : (e = new u(null, {
                        type: f
                        , size: i - t
                    }), e.detach(o.substr(t, e.size)), e)
                }
                i.call(this);
                f && this.connectRuntime(f);
                e ? n.typeOf(e) === "string" && (e = {
                    data: e
                }) : e = {};
                n.extend(this, {
                    uid: e.uid || n.guid("uid_")
                    , ruid: f
                    , size: e.size || 0
                    , type: e.type || ""
                    , slice: function (n, t, i) {
                        return this.isDetached() ? o.apply(this, arguments) : this.getRuntime()
                            .exec.call(this, "Blob", "slice", this.getSource(), n, t, i)
                    }
                    , getSource: function () {
                        return r[this.uid] ? r[this.uid] : null
                    }
                    , detach: function (n) {
                        if (this.ruid && (this.getRuntime()
                            .exec.call(this, "Blob", "destroy"), this.disconnectRuntime(), this.ruid = null), n = n || "", n.substr(0, 5) == "data:") {
                            var i = n.indexOf(";base64,");
                            this.type = n.substring(5, i);
                            n = t.atob(n.substring(i + 8))
                        }
                        this.size = n.length;
                        r[this.uid] = n
                    }
                    , isDetached: function () {
                        return !this.ruid && n.typeOf(r[this.uid]) === "string"
                    }
                    , destroy: function () {
                        this.detach();
                        delete r[this.uid]
                    }
                });
                e.data ? this.detach(e.data) : r[this.uid] = e
            }
            var r = {};
            return u
        });
        i("moxie/core/I18n", ["moxie/core/utils/Basic"], function (n) {
            var t = {};
            return {
                addI18n: function (i) {
                    return n.extend(t, i)
                }
                , translate: function (n) {
                    return t[n] || n
                }
                , _: function (n) {
                    return this.translate(n)
                }
                , sprintf: function (t) {
                    var i = [].slice.call(arguments, 1);
                    return t.replace(/%[a-z]/g, function () {
                        var t = i.shift();
                        return n.typeOf(t) !== "undefined" ? t : ""
                    })
                }
            }
        });
        i("moxie/core/utils/Mime", ["moxie/core/utils/Basic", "moxie/core/I18n"], function (n, t) {
            var r = {}
                , i = {}
                , u = function (n) {
                    for (var u = n.split(/,/), f, e, t = 0; t < u.length; t += 2) {
                        for (e = u[t + 1].split(/ /), f = 0; f < e.length; f++) r[e[f]] = u[t];
                        i[u[t]] = e
                    }
                }
                , o = function (t, i) {
                    for (var u, r, f, e = [], o = 0; o < t.length; o++)
                        for (u = t[o].extensions.toLowerCase()
                            .split(/\s*,\s*/), r = 0; r < u.length; r++) {
                            if (u[r] === "*") return [];
                            if (f = e[u[r]], i && /^\w+$/.test(u[r])) e.push("." + u[r]);
                            else if (f && n.inArray(f, e) === -1) e.push(f);
                            else if (!f) return []
                        }
                    return e
                }
                , f = function (t) {
                    var r = [];
                    return n.each(t, function (t) {
                        if (t = t.toLowerCase(), t === "*") return r = [], !1;
                        var u = t.match(/^(\w+)\/(\*|\w+)$/);
                        u && (u[2] === "*" ? n.each(i, function (n, t) {
                            new RegExp("^" + u[1] + "/")
                                .test(t) && [].push.apply(r, i[t])
                        }) : i[t] && [].push.apply(r, i[t]))
                    }), r
                }
                , s = function (i) {
                    var u = []
                        , r = [];
                    return n.typeOf(i) === "string" && (i = n.trim(i)
                        .split(/\s*,\s*/)), r = f(i), u.push({
                            title: t.translate("Files")
                            , extensions: r.length ? r.join(",") : "*"
                        }), u
                }
                , e = function (n) {
                    var t = n && n.match(/\.([^.]+)$/);
                    return t ? t[1].toLowerCase() : ""
                }
                , h = function (n) {
                    return r[e(n)] || ""
                };
            return u("application/msword,doc dot,application/pdf,pdf,application/pgp-signature,pgp,application/postscript,ps ai eps,application/rtf,rtf,application/vnd.ms-excel,xls xlb xlt xla,application/vnd.ms-powerpoint,ppt pps pot ppa,application/zip,zip,application/x-shockwave-flash,swf swfl,application/vnd.openxmlformats-officedocument.wordprocessingml.document,docx,application/vnd.openxmlformats-officedocument.wordprocessingml.template,dotx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,xlsx,application/vnd.openxmlformats-officedocument.presentationml.presentation,pptx,application/vnd.openxmlformats-officedocument.presentationml.template,potx,application/vnd.openxmlformats-officedocument.presentationml.slideshow,ppsx,application/x-javascript,js,application/json,json,audio/mpeg,mp3 mpga mpega mp2,audio/x-wav,wav,audio/x-m4a,m4a,audio/ogg,oga ogg,audio/aiff,aiff aif,audio/flac,flac,audio/aac,aac,audio/ac3,ac3,audio/x-ms-wma,wma,image/bmp,bmp,image/gif,gif,image/jpeg,jpg jpeg jpe,image/photoshop,psd,image/png,png,image/svg+xml,svg svgz,image/tiff,tiff tif,text/plain,asc txt text diff log,text/html,htm html xhtml,text/css,css,text/csv,csv,text/rtf,rtf,video/mpeg,mpeg mpg mpe m2v,video/quicktime,qt mov,video/mp4,mp4,video/x-m4v,m4v,video/x-flv,flv,video/x-ms-wmv,wmv,video/avi,avi,video/webm,webm,video/3gpp,3gpp 3gp,video/3gpp2,3g2,video/vnd.rn-realvideo,rv,video/ogg,ogv,video/x-matroska,mkv,application/vnd.oasis.opendocument.formula-template,otf,application/octet-stream,exe"), {
                mimes: r
                , extensions: i
                , addMimeType: u
                , extList2mimes: o
                , mimes2exts: f
                , mimes2extList: s
                , getFileExtension: e
                , getFileMime: h
            }
        });
        i("moxie/file/FileInput", ["moxie/core/utils/Basic", "moxie/core/utils/Env", "moxie/core/utils/Mime", "moxie/core/utils/Dom", "moxie/core/Exceptions", "moxie/core/EventTarget", "moxie/core/I18n", "moxie/runtime/Runtime", "moxie/runtime/RuntimeClient"], function (n, t, i, r, u, f, e, o, s) {
            function h(f) {
                MXI_DEBUG && t.log("Instantiating FileInput...");
                var h, l, a;
                if (n.inArray(n.typeOf(f), ["string", "node"]) !== -1 && (f = {
                    browse_button: f
                }), l = r.get(f.browse_button), !l) throw new u.DOMException(u.DOMException.NOT_FOUND_ERR);
                a = {
                    accept: [{
                        title: e.translate("All Files")
                        , extensions: "*"
                    }]
                    , multiple: !1
                    , required_caps: !1
                    , container: l.parentNode || document.body
                };
                f = n.extend({}, a, f);
                typeof f.required_caps == "string" && (f.required_caps = o.parseCaps(f.required_caps));
                typeof f.accept == "string" && (f.accept = i.mimes2extList(f.accept));
                h = r.get(f.container);
                h || (h = document.body);
                r.getStyle(h, "position") === "static" && (h.style.position = "relative");
                h = l = null;
                s.call(this);
                n.extend(this, {
                    uid: n.guid("uid_")
                    , ruid: null
                    , shimid: null
                    , files: null
                    , init: function () {
                        var t = this;
                        t.bind("RuntimeInit", function (i, u) {
                            t.ruid = u.uid;
                            t.shimid = u.shimid;
                            t.bind("Ready", function () {
                                t.trigger("Refresh")
                            }, 999);
                            t.bind("Refresh", function () {
                                var e, o, t, i, s;
                                t = r.get(f.browse_button);
                                i = r.get(u.shimid);
                                t && (e = r.getPos(t, r.get(f.container)), o = r.getSize(t), s = parseInt(r.getStyle(t, "z-index"), 10) || 0, i && n.extend(i.style, {
                                    top: e.y + "px"
                                    , left: e.x + "px"
                                    , width: o.w + "px"
                                    , height: o.h + "px"
                                    , zIndex: s + 1
                                }));
                                i = t = null
                            });
                            u.exec.call(t, "FileInput", "init", f)
                        });
                        t.connectRuntime(n.extend({}, f, {
                            required_caps: {
                                select_file: !0
                            }
                        }))
                    }
                    , getOption: function (n) {
                        return f[n]
                    }
                    , setOption: function (n, t) {
                        if (f.hasOwnProperty(n)) {
                            var r = f[n];
                            switch (n) {
                                case "accept":
                                    typeof t == "string" && (t = i.mimes2extList(t));
                                    break;
                                case "container":
                                case "required_caps":
                                    throw new u.FileException(u.FileException.NO_MODIFICATION_ALLOWED_ERR);
                            }
                            f[n] = t;
                            this.exec("FileInput", "setOption", n, t);
                            this.trigger("OptionChanged", n, t, r)
                        }
                    }
                    , disable: function (t) {
                        var i = this.getRuntime();
                        i && this.exec("FileInput", "disable", n.typeOf(t) === "undefined" ? !0 : t)
                    }
                    , refresh: function () {
                        this.trigger("Refresh")
                    }
                    , destroy: function () {
                        var t = this.getRuntime();
                        t && (t.exec.call(this, "FileInput", "destroy"), this.disconnectRuntime());
                        n.typeOf(this.files) === "array" && n.each(this.files, function (n) {
                            n.destroy()
                        });
                        this.files = null;
                        this.unbindAll()
                    }
                });
                this.handleEventProps(c)
            }
            var c = ["ready", "change", "cancel", "mouseenter", "mouseleave", "mousedown", "mouseup"];
            return h.prototype = f.instance, h
        });
        i("moxie/file/File", ["moxie/core/utils/Basic", "moxie/core/utils/Mime", "moxie/file/Blob"], function (n, t, i) {
            function r(r, u) {
                var f, e;
                u || (u = {});
                i.apply(this, arguments);
                this.type || (this.type = t.getFileMime(u.name));
                u.name ? (f = u.name.replace(/\\/g, "/"), f = f.substr(f.lastIndexOf("/") + 1)) : this.type && (e = this.type.split("/")[0], f = n.guid((e !== "" ? e : "file") + "_"), t.extensions[this.type] && (f += "." + t.extensions[this.type][0]));
                n.extend(this, {
                    name: f || n.guid("file_")
                    , relativePath: ""
                    , lastModifiedDate: u.lastModifiedDate || (new Date)
                        .toLocaleString()
                })
            }
            return r.prototype = i.prototype, r
        });
        i("moxie/file/FileDrop", ["moxie/core/I18n", "moxie/core/utils/Dom", "moxie/core/Exceptions", "moxie/core/utils/Basic", "moxie/core/utils/Env", "moxie/file/File", "moxie/runtime/RuntimeClient", "moxie/core/EventTarget", "moxie/core/utils/Mime"], function (n, t, i, r, u, f, e, o, s) {
            function h(i) {
                MXI_DEBUG && u.log("Instantiating FileDrop...");
                var f = this
                    , o;
                typeof i == "string" && (i = {
                    drop_zone: i
                });
                o = {
                    accept: [{
                        title: n.translate("All Files")
                        , extensions: "*"
                    }]
                    , required_caps: {
                        drag_and_drop: !0
                    }
                };
                i = typeof i == "object" ? r.extend({}, o, i) : o;
                i.container = t.get(i.drop_zone) || document.body;
                t.getStyle(i.container, "position") === "static" && (i.container.style.position = "relative");
                typeof i.accept == "string" && (i.accept = s.mimes2extList(i.accept));
                e.call(f);
                r.extend(f, {
                    uid: r.guid("uid_")
                    , ruid: null
                    , files: null
                    , init: function () {
                        f.bind("RuntimeInit", function (n, t) {
                            f.ruid = t.uid;
                            t.exec.call(f, "FileDrop", "init", i);
                            f.dispatchEvent("ready")
                        });
                        f.connectRuntime(i)
                    }
                    , destroy: function () {
                        var n = this.getRuntime();
                        n && (n.exec.call(this, "FileDrop", "destroy"), this.disconnectRuntime());
                        this.files = null;
                        this.unbindAll()
                    }
                });
                this.handleEventProps(c)
            }
            var c = ["ready", "dragenter", "dragleave", "drop", "error"];
            return h.prototype = o.instance, h
        });
        i("moxie/file/FileReader", ["moxie/core/utils/Basic", "moxie/core/utils/Encode", "moxie/core/Exceptions", "moxie/core/EventTarget", "moxie/file/Blob", "moxie/runtime/RuntimeClient"], function (n, t, i, r, u, f) {
            function e() {
                function r(n, r) {
                    var o = this
                        , f;
                    if (this.trigger("loadstart"), this.readyState === e.LOADING) {
                        this.trigger("error", new i.DOMException(i.DOMException.INVALID_STATE_ERR));
                        this.trigger("loadend");
                        return
                    }
                    if (!(r instanceof u)) {
                        this.trigger("error", new i.DOMException(i.DOMException.NOT_FOUND_ERR));
                        this.trigger("loadend");
                        return
                    }
                    if (this.result = null, this.readyState = e.LOADING, r.isDetached()) {
                        f = r.getSource();
                        switch (n) {
                            case "readAsText":
                            case "readAsBinaryString":
                                this.result = f;
                                break;
                            case "readAsDataURL":
                                this.result = "data:" + r.type + ";base64," + t.btoa(f)
                        }
                        this.readyState = e.DONE;
                        this.trigger("load");
                        this.trigger("loadend")
                    } else this.connectRuntime(r.ruid), this.exec("FileReader", "read", n, r)
                }
                f.call(this);
                n.extend(this, {
                    uid: n.guid("uid_")
                    , readyState: e.EMPTY
                    , result: null
                    , error: null
                    , readAsBinaryString: function (n) {
                        r.call(this, "readAsBinaryString", n)
                    }
                    , readAsDataURL: function (n) {
                        r.call(this, "readAsDataURL", n)
                    }
                    , readAsText: function (n) {
                        r.call(this, "readAsText", n)
                    }
                    , abort: function () {
                        (this.result = null, n.inArray(this.readyState, [e.EMPTY, e.DONE]) === -1) && (this.readyState === e.LOADING && (this.readyState = e.DONE), this.exec("FileReader", "abort"), this.trigger("abort"), this.trigger("loadend"))
                    }
                    , destroy: function () {
                        this.abort();
                        this.exec("FileReader", "destroy");
                        this.disconnectRuntime();
                        this.unbindAll()
                    }
                });
                this.handleEventProps(o);
                this.bind("Error", function (n, t) {
                    this.readyState = e.DONE;
                    this.error = t
                }, 999);
                this.bind("Load", function () {
                    this.readyState = e.DONE
                }, 999)
            }
            var o = ["loadstart", "progress", "load", "abort", "error", "loadend"];
            return e.EMPTY = 0, e.LOADING = 1, e.DONE = 2, e.prototype = r.instance, e
        });
        i("moxie/core/utils/Url", ["moxie/core/utils/Basic"], function (n) {
            var t = function (i, r) {
                var s = ["source", "scheme", "authority", "userInfo", "user", "pass", "host", "port", "relative", "path", "directory", "file", "query", "fragment"]
                    , e = s.length
                    , u = {}
                    , h = /^(?:([^:\/?#]+):)?(?:\/\/()(?:(?:()(?:([^:@\/]*):?([^:@\/]*))?@)?(\[[\da-fA-F:]+\]|[^:\/?#]*)(?::(\d*))?))?()(?:(()(?:(?:[^?#\/]*\/)*)()(?:[^?#]*))(?:\\?([^#]*))?(?:#(.*))?)/.exec(i || "")
                    , o, c = /^\/\/\w/.test(i)
                    , f;
                switch (n.typeOf(r)) {
                    case "undefined":
                        r = t(document.location.href, !1);
                        break;
                    case "string":
                        r = t(r, !1)
                }
                while (e--) h[e] && (u[s[e]] = h[e]);
                return o = !c && !u.scheme, (c || o) && (u.scheme = r.scheme), o && (u.host = r.host, u.port = r.port, f = "", /^[^\/]/.test(u.path) && (f = r.path, f = /\/[^\/]*\.[^\/]*$/.test(f) ? f.replace(/\/[^\/]+$/, "/") : f.replace(/\/?$/, "/")), u.path = f + (u.path || "")), u.port || (u.port = {
                    http: 80
                    , https: 443
                }[u.scheme] || 80), u.port = parseInt(u.port, 10), u.path || (u.path = "/"), delete u.source, u
            }
                , i = function (n) {
                    var i = typeof n == "object" ? n : t(n);
                    return i.scheme + "://" + i.host + (i.port !== {
                        http: 80
                        , https: 443
                    }[i.scheme] ? ":" + i.port : "") + i.path + (i.query ? i.query : "")
                }
                , r = function (n) {
                    function i(n) {
                        return [n.scheme, n.host, n.port].join("/")
                    }
                    return typeof n == "string" && (n = t(n)), i(t()) === i(n)
                };
            return {
                parseUrl: t
                , resolveUrl: i
                , hasSameOrigin: r
            }
        });
        i("moxie/runtime/RuntimeTarget", ["moxie/core/utils/Basic", "moxie/runtime/RuntimeClient", "moxie/core/EventTarget"], function (n, t, i) {
            function r() {
                this.uid = n.guid("uid_");
                t.call(this);
                this.destroy = function () {
                    this.disconnectRuntime();
                    this.unbindAll()
                }
            }
            return r.prototype = i.instance, r
        });
        i("moxie/file/FileReaderSync", ["moxie/core/utils/Basic", "moxie/runtime/RuntimeClient", "moxie/core/utils/Encode"], function (n, t, i) {
            return function () {
                function r(n, t) {
                    var r, f, u, e, o;
                    if (t.isDetached()) {
                        r = t.getSource();
                        switch (n) {
                            case "readAsBinaryString":
                                return r;
                            case "readAsDataURL":
                                return "data:" + t.type + ";base64," + i.btoa(r);
                            case "readAsText":
                                for (f = "", u = 0, e = r.length; u < e; u++) f += String.fromCharCode(r[u]);
                                return f
                        }
                    } else return o = this.connectRuntime(t.ruid)
                        .exec.call(this, "FileReaderSync", "read", n, t), this.disconnectRuntime(), o
                }
                t.call(this);
                n.extend(this, {
                    uid: n.guid("uid_")
                    , readAsBinaryString: function (n) {
                        return r.call(this, "readAsBinaryString", n)
                    }
                    , readAsDataURL: function (n) {
                        return r.call(this, "readAsDataURL", n)
                    }
                    , readAsText: function (n) {
                        return r.call(this, "readAsText", n)
                    }
                })
            }
        });
        i("moxie/xhr/FormData", ["moxie/core/Exceptions", "moxie/core/utils/Basic", "moxie/file/Blob"], function (n, t, i) {
            function r() {
                var n, r = [];
                t.extend(this, {
                    append: function (u, f) {
                        var o = this
                            , e = t.typeOf(f);
                        f instanceof i ? n = {
                            name: u
                            , value: f
                        } : "array" === e ? (u += "[]", t.each(f, function (n) {
                            o.append(u, n)
                        })) : "object" === e ? t.each(f, function (n, t) {
                            o.append(u + "[" + t + "]", n)
                        }) : "null" === e || "undefined" === e || "number" === e && isNaN(f) ? o.append(u, "false") : r.push({
                            name: u
                            , value: f.toString()
                        })
                    }
                    , hasBlob: function () {
                        return !!this.getBlob()
                    }
                    , getBlob: function () {
                        return n && n.value || null
                    }
                    , getBlobName: function () {
                        return n && n.name || null
                    }
                    , each: function (i) {
                        t.each(r, function (n) {
                            i(n.value, n.name)
                        });
                        n && i(n.value, n.name)
                    }
                    , destroy: function () {
                        n = null;
                        r = []
                    }
                })
            }
            return r
        });
        i("moxie/xhr/XMLHttpRequest", ["moxie/core/utils/Basic", "moxie/core/Exceptions", "moxie/core/EventTarget", "moxie/core/utils/Encode", "moxie/core/utils/Url", "moxie/runtime/Runtime", "moxie/runtime/RuntimeTarget", "moxie/file/Blob", "moxie/file/FileReaderSync", "moxie/xhr/FormData", "moxie/core/utils/Env", "moxie/core/utils/Mime"], function (n, t, i, r, u, f, e, o, s, h, c, l) {
            function v() {
                this.uid = n.guid("uid_")
            }

            function a() {
                function i(n, t) {
                    if (tt.hasOwnProperty(n)) {
                        if (arguments.length === 1) return c.can("define_property") ? tt[n] : ht[n];
                        c.can("define_property") ? tt[n] = t : ht[n] = t
                    }
                }

                function kt(t) {
                    function u() {
                        s && (s.destroy(), s = null);
                        r.dispatchEvent("loadend");
                        r = null
                    }

                    function o(f) {
                        s.bind("LoadStart", function (n) {
                            i("readyState", a.LOADING);
                            r.dispatchEvent("readystatechange");
                            r.dispatchEvent(n);
                            ft && r.upload.dispatchEvent(n)
                        });
                        s.bind("Progress", function (n) {
                            i("readyState") !== a.LOADING && (i("readyState", a.LOADING), r.dispatchEvent("readystatechange"));
                            r.dispatchEvent(n)
                        });
                        s.bind("UploadProgress", function (n) {
                            ft && r.upload.dispatchEvent({
                                type: "progress"
                                , lengthComputable: !1
                                , total: n.total
                                , loaded: n.loaded
                            })
                        });
                        s.bind("Load", function (t) {
                            i("readyState", a.DONE);
                            i("status", Number(f.exec.call(s, "XMLHttpRequest", "getStatus") || 0));
                            i("statusText", p[i("status")] || "");
                            i("response", f.exec.call(s, "XMLHttpRequest", "getResponse", i("responseType")));
                            ~n.inArray(i("responseType"), ["text", ""]) ? i("responseText", i("response")) : i("responseType") === "document" && i("responseXML", i("response"));
                            nt = f.exec.call(s, "XMLHttpRequest", "getAllResponseHeaders");
                            r.dispatchEvent("readystatechange");
                            i("status") > 0 ? (ft && r.upload.dispatchEvent(t), r.dispatchEvent(t)) : (g = !0, r.dispatchEvent("error"));
                            u()
                        });
                        s.bind("Abort", function (n) {
                            r.dispatchEvent(n);
                            u()
                        });
                        s.bind("Error", function (n) {
                            g = !0;
                            i("readyState", a.DONE);
                            r.dispatchEvent("readystatechange");
                            et = !0;
                            r.dispatchEvent(n);
                            u()
                        });
                        f.exec.call(s, "XMLHttpRequest", "send", {
                            url: ct
                            , method: st
                            , async: it
                            , user: lt
                            , password: at
                            , headers: b
                            , mimeType: rt
                            , encoding: vt
                            , responseType: r.responseType
                            , withCredentials: r.withCredentials
                            , options: w
                        }, t)
                    }
                    var r = this;
                    yt = (new Date)
                        .getTime();
                    s = new e;
                    typeof w.required_caps == "string" && (w.required_caps = f.parseCaps(w.required_caps));
                    w.required_caps = n.extend({}, w.required_caps, {
                        return_response_type: r.responseType
                    });
                    t instanceof h && (w.required_caps.send_multipart = !0);
                    n.isEmptyObj(b) || (w.required_caps.send_custom_headers = !0);
                    ot || (w.required_caps.do_cors = !0);
                    w.ruid ? o(s.connectRuntime(w)) : (s.bind("RuntimeInit", function (n, t) {
                        o(t)
                    }), s.bind("RuntimeError", function (n, t) {
                        r.dispatchEvent("RuntimeError", t)
                    }), s.connectRuntime(w))
                }

                function dt() {
                    i("responseText", "");
                    i("responseXML", null);
                    i("response", null);
                    i("status", 0);
                    i("statusText", "");
                    yt = pt = null
                }
                var ht = this
                    , tt = {
                        timeout: 0
                        , readyState: a.UNSENT
                        , withCredentials: !1
                        , status: 0
                        , statusText: ""
                        , responseType: ""
                        , responseXML: null
                        , responseText: null
                        , response: null
                    }
                    , it = !0
                    , ct, st, b = {}
                    , lt, at, vt = null
                    , rt = null
                    , ut = !1
                    , d = !1
                    , ft = !1
                    , et = !1
                    , g = !1
                    , ot = !1
                    , yt, pt, wt = null
                    , bt = null
                    , w = {}
                    , s, nt = ""
                    , k;
                n.extend(this, tt, {
                    uid: n.guid("uid_")
                    , upload: new v
                    , open: function (f, e, o, s, h) {
                        var c;
                        if (!f || !e) throw new t.DOMException(t.DOMException.SYNTAX_ERR);
                        if (/[\u0100-\uffff]/.test(f) || r.utf8_encode(f) !== f) throw new t.DOMException(t.DOMException.SYNTAX_ERR);
                        if (!~n.inArray(f.toUpperCase(), ["CONNECT", "DELETE", "GET", "HEAD", "OPTIONS", "POST", "PUT", "TRACE", "TRACK"]) || (st = f.toUpperCase()), !!~n.inArray(st, ["CONNECT", "TRACE", "TRACK"])) throw new t.DOMException(t.DOMException.SECURITY_ERR);
                        if (e = r.utf8_encode(e), c = u.parseUrl(e), ot = u.hasSameOrigin(c), ct = u.resolveUrl(e), (s || h) && !ot) throw new t.DOMException(t.DOMException.INVALID_ACCESS_ERR);
                        if (lt = s || c.user, at = h || c.pass, it = o || !0, it === !1 && (i("timeout") || i("withCredentials") || i("responseType") !== "")) throw new t.DOMException(t.DOMException.INVALID_ACCESS_ERR);
                        ut = !it;
                        d = !1;
                        b = {};
                        dt.call(this);
                        i("readyState", a.OPENED);
                        this.dispatchEvent("readystatechange")
                    }
                    , setRequestHeader: function (u, f) {
                        if (i("readyState") !== a.OPENED || d) throw new t.DOMException(t.DOMException.INVALID_STATE_ERR);
                        if (/[\u0100-\uffff]/.test(u) || r.utf8_encode(u) !== u) throw new t.DOMException(t.DOMException.SYNTAX_ERR);
                        return (u = n.trim(u)
                            .toLowerCase(), !!~n.inArray(u, ["accept-charset", "accept-encoding", "access-control-request-headers", "access-control-request-method", "connection", "content-length", "cookie", "cookie2", "content-transfer-encoding", "date", "expect", "host", "keep-alive", "origin", "referer", "te", "trailer", "transfer-encoding", "upgrade", "user-agent", "via"]) || /^(proxy\-|sec\-)/.test(u)) ? !1 : (b[u] ? b[u] += ", " + f : b[u] = f, !0)
                    }
                    , hasRequestHeader: function (n) {
                        return n && b[n.toLowerCase()] || !1
                    }
                    , getAllResponseHeaders: function () {
                        return nt || ""
                    }
                    , getResponseHeader: function (t) {
                        return (t = t.toLowerCase(), g || !!~n.inArray(t, ["set-cookie", "set-cookie2"])) ? null : nt && nt !== "" && (k || (k = {}, n.each(nt.split(/\r\n/), function (t) {
                            var i = t.split(/:\s+/);
                            i.length === 2 && (i[0] = n.trim(i[0]), k[i[0].toLowerCase()] = {
                                header: i[0]
                                , value: n.trim(i[1])
                            })
                        })), k.hasOwnProperty(t)) ? k[t].header + ": " + k[t].value : null
                    }
                    , overrideMimeType: function (r) {
                        var u, f;
                        if (!!~n.inArray(i("readyState"), [a.LOADING, a.DONE])) throw new t.DOMException(t.DOMException.INVALID_STATE_ERR);
                        if (r = n.trim(r.toLowerCase()), /;/.test(r) && (u = r.match(/^([^;]+)(?:;\scharset\=)?(.*)$/)) && (r = u[1], u[2] && (f = u[2])), !l.mimes[r]) throw new t.DOMException(t.DOMException.SYNTAX_ERR);
                        wt = r;
                        bt = f
                    }
                    , send: function (i, u) {
                        if (w = n.typeOf(u) === "string" ? {
                            ruid: u
                        } : u ? u : {}, this.readyState !== a.OPENED || d) throw new t.DOMException(t.DOMException.INVALID_STATE_ERR);
                        if (i instanceof o) w.ruid = i.ruid, rt = i.type || "application/octet-stream";
                        else if (i instanceof h) {
                            if (i.hasBlob()) {
                                var f = i.getBlob();
                                w.ruid = f.ruid;
                                rt = f.type || "application/octet-stream"
                            }
                        } else typeof i == "string" && (vt = "UTF-8", rt = "text/plain;charset=UTF-8", i = r.utf8_encode(i));
                        this.withCredentials || (this.withCredentials = w.required_caps && w.required_caps.send_browser_cookies && !ot);
                        ft = !ut && this.upload.hasEventListener();
                        g = !1;
                        et = !i;
                        ut || (d = !0);
                        kt.call(this, i)
                    }
                    , abort: function () {
                        if (g = !0, ut = !1, ~n.inArray(i("readyState"), [a.UNSENT, a.OPENED, a.DONE])) i("readyState", a.UNSENT);
                        else {
                            if (i("readyState", a.DONE), d = !1, s) s.getRuntime()
                                .exec.call(s, "XMLHttpRequest", "abort", et);
                            else throw new t.DOMException(t.DOMException.INVALID_STATE_ERR);
                            et = !0
                        }
                    }
                    , destroy: function () {
                        s && (n.typeOf(s.destroy) === "function" && s.destroy(), s = null);
                        this.unbindAll();
                        this.upload && (this.upload.unbindAll(), this.upload = null)
                    }
                });
                this.handleEventProps(y.concat(["readystatechange"]));
                this.upload.handleEventProps(y)
            }
            var p = {
                100: "Continue"
                , 101: "Switching Protocols"
                , 102: "Processing"
                , 200: "OK"
                , 201: "Created"
                , 202: "Accepted"
                , 203: "Non-Authoritative Information"
                , 204: "No Content"
                , 205: "Reset Content"
                , 206: "Partial Content"
                , 207: "Multi-Status"
                , 226: "IM Used"
                , 300: "Multiple Choices"
                , 301: "Moved Permanently"
                , 302: "Found"
                , 303: "See Other"
                , 304: "Not Modified"
                , 305: "Use Proxy"
                , 306: "Reserved"
                , 307: "Temporary Redirect"
                , 400: "Bad Request"
                , 401: "Unauthorized"
                , 402: "Payment Required"
                , 403: "Forbidden"
                , 404: "Not Found"
                , 405: "Method Not Allowed"
                , 406: "Not Acceptable"
                , 407: "Proxy Authentication Required"
                , 408: "Request Timeout"
                , 409: "Conflict"
                , 410: "Gone"
                , 411: "Length Required"
                , 412: "Precondition Failed"
                , 413: "Request Entity Too Large"
                , 414: "Request-URI Too Long"
                , 415: "Unsupported Media Type"
                , 416: "Requested Range Not Satisfiable"
                , 417: "Expectation Failed"
                , 422: "Unprocessable Entity"
                , 423: "Locked"
                , 424: "Failed Dependency"
                , 426: "Upgrade Required"
                , 500: "Internal Server Error"
                , 501: "Not Implemented"
                , 502: "Bad Gateway"
                , 503: "Service Unavailable"
                , 504: "Gateway Timeout"
                , 505: "HTTP Version Not Supported"
                , 506: "Variant Also Negotiates"
                , 507: "Insufficient Storage"
                , 510: "Not Extended"
            };
            v.prototype = i.instance;
            var y = ["loadstart", "progress", "abort", "error", "load", "timeout", "loadend"];
            return a.UNSENT = 0, a.OPENED = 1, a.HEADERS_RECEIVED = 2, a.LOADING = 3, a.DONE = 4, a.prototype = i.instance, a
        });
        i("moxie/runtime/Transporter", ["moxie/core/utils/Basic", "moxie/core/utils/Encode", "moxie/runtime/RuntimeClient", "moxie/core/EventTarget"], function (n, t, i, r) {
            function u() {
                function h() {
                    f = e = 0;
                    o = this.result = null
                }

                function l(t, i) {
                    var s = this;
                    r = i;
                    s.bind("TransportingProgress", function (t) {
                        e = t.loaded;
                        e < f && n.inArray(s.state, [u.IDLE, u.DONE]) === -1 && a.call(s)
                    }, 999);
                    s.bind("TransportingComplete", function () {
                        e = f;
                        s.state = u.DONE;
                        o = null;
                        s.result = r.exec.call(s, "Transporter", "getAsBlob", t || "")
                    }, 999);
                    s.state = u.BUSY;
                    s.trigger("TransportingStarted");
                    a.call(s)
                }

                function a() {
                    var u = this
                        , n, i = f - e;
                    s > i && (s = i);
                    n = t.btoa(o.substr(e, s));
                    r.exec.call(u, "Transporter", "receive", n, f)
                }
                var c, r, o, f, e, s;
                i.call(this);
                n.extend(this, {
                    uid: n.guid("uid_")
                    , state: u.IDLE
                    , result: null
                    , transport: function (t, i, r) {
                        var u = this
                            , e;
                        r = n.extend({
                            chunk_size: 204798
                        }, r);
                        (c = r.chunk_size % 3) && (r.chunk_size += 3 - c);
                        s = r.chunk_size;
                        h.call(this);
                        o = t;
                        f = t.length;
                        n.typeOf(r) === "string" || r.ruid ? l.call(u, i, this.connectRuntime(r)) : (e = function (n, t) {
                            u.unbind("RuntimeInit", e);
                            l.call(u, i, t)
                        }, this.bind("RuntimeInit", e), this.connectRuntime(r))
                    }
                    , abort: function () {
                        var n = this;
                        n.state = u.IDLE;
                        r && (r.exec.call(n, "Transporter", "clear"), n.trigger("TransportingAborted"));
                        h.call(n)
                    }
                    , destroy: function () {
                        this.unbindAll();
                        r = null;
                        this.disconnectRuntime();
                        h.call(this)
                    }
                })
            }
            return u.IDLE = 0, u.BUSY = 1, u.DONE = 2, u.prototype = r.instance, u
        });
        i("moxie/image/Image", ["moxie/core/utils/Basic", "moxie/core/utils/Dom", "moxie/core/Exceptions", "moxie/file/FileReaderSync", "moxie/xhr/XMLHttpRequest", "moxie/runtime/Runtime", "moxie/runtime/RuntimeClient", "moxie/runtime/Transporter", "moxie/core/utils/Env", "moxie/core/EventTarget", "moxie/file/Blob", "moxie/file/File", "moxie/core/utils/Encode"], function (n, t, i, r, u, f, e, o, s, h, c, l, a) {
            function v() {
                function p(n) {
                    try {
                        return n || (n = this.exec("Image", "getInfo")), this.size = n.size, this.width = n.width, this.height = n.height, this.type = n.type, this.meta = n.meta, this.name === "" && (this.name = n.name), !0
                    } catch (t) {
                        return this.trigger("error", t.code), !1
                    }
                }

                function r(t) {
                    var u = n.typeOf(t);
                    try {
                        if (t instanceof v) {
                            if (!t.size) throw new i.DOMException(i.DOMException.INVALID_STATE_ERR);
                            w.apply(this, arguments)
                        } else if (t instanceof c) {
                            if (!~n.inArray(t.type, ["image/jpeg", "image/png"])) throw new i.ImageError(i.ImageError.WRONG_FORMAT);
                            h.apply(this, arguments)
                        } else if (n.inArray(u, ["blob", "file"]) !== -1) r.call(this, new l(null, t), arguments[1]);
                        else if (u === "string") t.substr(0, 5) === "data:" ? r.call(this, new c(null, {
                            data: t
                        }), arguments[1]) : b.apply(this, arguments);
                        else if (u === "node" && t.nodeName.toLowerCase() === "img") r.call(this, t.src, arguments[1]);
                        else throw new i.DOMException(i.DOMException.TYPE_MISMATCH_ERR);
                    } catch (f) {
                        this.trigger("error", f.code)
                    }
                }

                function w(t, i) {
                    var r = this.connectRuntime(t.ruid);
                    this.ruid = r.uid;
                    r.exec.call(this, "Image", "loadFromImage", t, n.typeOf(i) === "undefined" ? !0 : i)
                }

                function h(t, i) {
                    function u(n) {
                        r.ruid = n.uid;
                        n.exec.call(r, "Image", "loadFromBlob", t)
                    }
                    var r = this;
                    r.name = t.name || "";
                    t.isDetached() ? (this.bind("RuntimeInit", function (n, t) {
                        u(t)
                    }), i && typeof i.required_caps == "string" && (i.required_caps = f.parseCaps(i.required_caps)), this.connectRuntime(n.extend({
                        required_caps: {
                            access_image_binary: !0
                            , resize_image: !0
                        }
                    }, i))) : u(this.connectRuntime(t.ruid))
                }

                function b(n, t) {
                    var r = this
                        , i;
                    i = new u;
                    i.open("get", n);
                    i.responseType = "blob";
                    i.onprogress = function (n) {
                        r.trigger(n)
                    };
                    i.onload = function () {
                        h.call(r, i.response, !0)
                    };
                    i.onerror = function (n) {
                        r.trigger(n)
                    };
                    i.onloadend = function () {
                        i.destroy()
                    };
                    i.bind("RuntimeError", function (n, t) {
                        r.trigger("RuntimeError", t)
                    });
                    i.send(null, t)
                }
                e.call(this);
                n.extend(this, {
                    uid: n.guid("uid_")
                    , ruid: null
                    , name: ""
                    , size: 0
                    , width: 0
                    , height: 0
                    , type: ""
                    , meta: {}
                    , clone: function () {
                        this.load.apply(this, arguments)
                    }
                    , load: function () {
                        r.apply(this, arguments)
                    }
                    , resize: function (t) {
                        var u = this
                            , o, e, r = {
                                x: 0
                                , y: 0
                                , width: u.width
                                , height: u.height
                            }
                            , f = n.extendIf({
                                width: u.width
                                , height: u.height
                                , type: u.type || "image/jpeg"
                                , quality: 90
                                , crop: !1
                                , fit: !0
                                , preserveHeaders: !0
                                , resample: "default"
                                , multipass: !0
                            }, t)
                            , s;
                        try {
                            if (!u.size) throw new i.DOMException(i.DOMException.INVALID_STATE_ERR);
                            if (u.width > v.MAX_RESIZE_WIDTH || u.height > v.MAX_RESIZE_HEIGHT) throw new i.ImageError(i.ImageError.MAX_RESOLUTION_ERR);
                            if (o = u.meta && u.meta.tiff && u.meta.tiff.Orientation || 1, n.inArray(o, [5, 6, 7, 8]) !== -1 && (s = f.width, f.width = f.height, f.height = s), f.crop) {
                                e = Math.max(f.width / u.width, f.height / u.height);
                                t.fit ? (r.width = Math.min(Math.ceil(f.width / e), u.width), r.height = Math.min(Math.ceil(f.height / e), u.height), e = f.width / r.width) : (r.width = Math.min(f.width, u.width), r.height = Math.min(f.height, u.height), e = 1);
                                typeof f.crop == "boolean" && (f.crop = "cc");
                                switch (f.crop.toLowerCase()
                                    .replace(/_/, "-")) {
                                    case "rb":
                                    case "right-bottom":
                                        r.x = u.width - r.width;
                                        r.y = u.height - r.height;
                                        break;
                                    case "cb":
                                    case "center-bottom":
                                        r.x = Math.floor((u.width - r.width) / 2);
                                        r.y = u.height - r.height;
                                        break;
                                    case "lb":
                                    case "left-bottom":
                                        r.x = 0;
                                        r.y = u.height - r.height;
                                        break;
                                    case "lt":
                                    case "left-top":
                                        r.x = 0;
                                        r.y = 0;
                                        break;
                                    case "ct":
                                    case "center-top":
                                        r.x = Math.floor((u.width - r.width) / 2);
                                        r.y = 0;
                                        break;
                                    case "rt":
                                    case "right-top":
                                        r.x = u.width - r.width;
                                        r.y = 0;
                                        break;
                                    case "rc":
                                    case "right-center":
                                    case "right-middle":
                                        r.x = u.width - r.width;
                                        r.y = Math.floor((u.height - r.height) / 2);
                                        break;
                                    case "lc":
                                    case "left-center":
                                    case "left-middle":
                                        r.x = 0;
                                        r.y = Math.floor((u.height - r.height) / 2);
                                        break;
                                    case "cc":
                                    case "center-center":
                                    case "center-middle":
                                    default:
                                        r.x = Math.floor((u.width - r.width) / 2);
                                        r.y = Math.floor((u.height - r.height) / 2)
                                }
                                r.x = Math.max(r.x, 0);
                                r.y = Math.max(r.y, 0)
                            } else e = Math.min(f.width / u.width, f.height / u.height), e > 1 && !f.fit && (e = 1);
                            this.exec("Image", "resize", r, e, f)
                        } catch (h) {
                            u.trigger("error", h.code)
                        }
                    }
                    , downsize: function (t) {
                        var i = {
                            width: this.width
                            , height: this.height
                            , type: this.type || "image/jpeg"
                            , quality: 90
                            , crop: !1
                            , fit: !1
                            , preserveHeaders: !0
                            , resample: "default"
                        }
                            , r;
                        r = typeof t == "object" ? n.extend(i, t) : n.extend(i, {
                            width: arguments[0]
                            , height: arguments[1]
                            , crop: arguments[2]
                            , preserveHeaders: arguments[3]
                        });
                        this.resize(r)
                    }
                    , crop: function (n, t, i) {
                        this.downsize(n, t, !0, i)
                    }
                    , getAsCanvas: function () {
                        if (!s.can("create_canvas")) throw new i.RuntimeError(i.RuntimeError.NOT_SUPPORTED_ERR);
                        return this.exec("Image", "getAsCanvas")
                    }
                    , getAsBlob: function (n, t) {
                        if (!this.size) throw new i.DOMException(i.DOMException.INVALID_STATE_ERR);
                        return this.exec("Image", "getAsBlob", n || "image/jpeg", t || 90)
                    }
                    , getAsDataURL: function (n, t) {
                        if (!this.size) throw new i.DOMException(i.DOMException.INVALID_STATE_ERR);
                        return this.exec("Image", "getAsDataURL", n || "image/jpeg", t || 90)
                    }
                    , getAsBinaryString: function (n, t) {
                        var i = this.getAsDataURL(n, t);
                        return a.atob(i.substring(i.indexOf("base64,") + 7))
                    }
                    , embed: function (r, u) {
                        function l(t, u) {
                            var f = this
                                , l, h, v;
                            if (s.can("create_canvas") && (l = f.getAsCanvas(), l)) {
                                r.appendChild(l);
                                l = null;
                                f.destroy();
                                e.trigger("embedded");
                                return
                            }
                            if (h = f.getAsDataURL(t, u), !h) throw new i.ImageError(i.ImageError.WRONG_FORMAT);
                            s.can("use_data_uri_of", h.length) ? (r.innerHTML = '<img src="' + h + '" width="' + f.width + '" height="' + f.height + '" alt="" />', f.destroy(), e.trigger("embedded")) : (v = new o, v.bind("TransportingComplete", function () {
                                c = e.connectRuntime(this.result.ruid);
                                e.bind("Embedded", function () {
                                    n.extend(c.getShimContainer()
                                        .style, {
                                        top: "0px"
                                        , left: "0px"
                                        , width: f.width + "px"
                                        , height: f.height + "px"
                                    });
                                    c = null
                                }, 999);
                                c.exec.call(e, "ImageView", "display", this.result.uid, width, height);
                                f.destroy()
                            }), v.transport(a.atob(h.substring(h.indexOf("base64,") + 7)), t, {
                                required_caps: {
                                    display_media: !0
                                }
                                , runtime_order: "flash,silverlight"
                                , container: r
                            }))
                        }
                        var e = this
                            , c, h = n.extend({
                                width: this.width
                                , height: this.height
                                , type: this.type || "image/jpeg"
                                , quality: 90
                                , fit: !0
                                , resample: "nearest"
                            }, u)
                            , f;
                        try {
                            if (!(r = t.get(r))) throw new i.DOMException(i.DOMException.INVALID_NODE_TYPE_ERR);
                            if (!this.size) throw new i.DOMException(i.DOMException.INVALID_STATE_ERR);
                            return this.width > v.MAX_RESIZE_WIDTH || this.height > v.MAX_RESIZE_HEIGHT, f = new v, f.bind("Resize", function () {
                                l.call(this, h.type, h.quality)
                            }), f.bind("Load", function () {
                                this.downsize(h)
                            }), this.meta.thumb && this.meta.thumb.width >= h.width && this.meta.thumb.height >= h.height ? f.load(this.meta.thumb.data) : f.clone(this, !1), f
                        } catch (y) {
                            this.trigger("error", y.code)
                        }
                    }
                    , destroy: function () {
                        this.ruid && (this.getRuntime()
                            .exec.call(this, "Image", "destroy"), this.disconnectRuntime());
                        this.meta && this.meta.thumb && this.meta.thumb.data.destroy();
                        this.unbindAll()
                    }
                });
                this.handleEventProps(y);
                this.bind("Load Resize", function () {
                    return p.call(this)
                }, 999)
            }
            var y = ["progress", "load", "error", "resize", "embedded"];
            return v.MAX_RESIZE_WIDTH = 8192, v.MAX_RESIZE_HEIGHT = 8192, v.prototype = h.instance, v
        });
        i("moxie/runtime/html5/Runtime", ["moxie/core/utils/Basic", "moxie/core/Exceptions", "moxie/runtime/Runtime", "moxie/core/utils/Env"], function (n, t, i, r) {
            function o(t) {
                var o = this
                    , s = i.capTest
                    , h = i.capTrue
                    , c = n.extend({
                        access_binary: s(window.FileReader || window.File && window.File.getAsDataURL)
                        , access_image_binary: function () {
                            return o.can("access_binary") && !!f.Image
                        }
                        , display_media: s((r.can("create_canvas") || r.can("use_data_uri_over32kb")) && u("moxie/image/Image"))
                        , do_cors: s(window.XMLHttpRequest && "withCredentials" in new XMLHttpRequest)
                        , drag_and_drop: s(function () {
                            var n = document.createElement("div");
                            return ("draggable" in n || "ondragstart" in n && "ondrop" in n) && (r.browser !== "IE" || r.verComp(r.version, 9, ">"))
                        }())
                        , filter_by_extension: s(function () {
                            return !(r.browser === "Chrome" && r.verComp(r.version, 28, "<") || r.browser === "IE" && r.verComp(r.version, 10, "<") || r.browser === "Safari" && r.verComp(r.version, 7, "<") || r.browser === "Firefox" && r.verComp(r.version, 37, "<"))
                        }())
                        , return_response_headers: h
                        , return_response_type: function (n) {
                            return n === "json" && !!window.JSON ? !0 : r.can("return_response_type", n)
                        }
                        , return_status_code: h
                        , report_upload_progress: s(window.XMLHttpRequest && (new XMLHttpRequest)
                            .upload)
                        , resize_image: function () {
                            return o.can("access_binary") && r.can("create_canvas")
                        }
                        , select_file: function () {
                            return r.can("use_fileinput") && window.File
                        }
                        , select_folder: function () {
                            return o.can("select_file") && (r.browser === "Chrome" && r.verComp(r.version, 21, ">=") || r.browser === "Firefox" && r.verComp(r.version, 42, ">="))
                        }
                        , select_multiple: function () {
                            return o.can("select_file") && !(r.browser === "Safari" && r.os === "Windows") && !(r.os === "iOS" && r.verComp(r.osVersion, "7.0.0", ">") && r.verComp(r.osVersion, "8.0.0", "<"))
                        }
                        , send_binary_string: s(window.XMLHttpRequest && ((new XMLHttpRequest)
                            .sendAsBinary || window.Uint8Array && window.ArrayBuffer))
                        , send_custom_headers: s(window.XMLHttpRequest)
                        , send_multipart: function () {
                            return !!(window.XMLHttpRequest && (new XMLHttpRequest)
                                .upload && window.FormData) || o.can("send_binary_string")
                        }
                        , slice_blob: s(window.File && (File.prototype.mozSlice || File.prototype.webkitSlice || File.prototype.slice))
                        , stream_upload: function () {
                            return o.can("slice_blob") && o.can("send_multipart")
                        }
                        , summon_file_dialog: function () {
                            return o.can("select_file") && !(r.browser === "Firefox" && r.verComp(r.version, 4, "<") || r.browser === "Opera" && r.verComp(r.version, 12, "<") || r.browser === "IE" && r.verComp(r.version, 10, "<") || r.os === "iOS" || r.os === "Android")
                        }
                        , upload_filesize: h
                        , use_http_method: h
                    }, arguments[2]);
                i.call(this, t, arguments[1] || e, c);
                n.extend(this, {
                    init: function () {
                        this.trigger("Init")
                    }
                    , destroy: function (n) {
                        return function () {
                            n.call(o);
                            n = o = null
                        }
                    }(this.destroy)
                });
                n.extend(this.getShim(), f)
            }
            var e = "html5"
                , f = {};
            return i.addConstructor(e, o), f
        });
        i("moxie/runtime/html5/file/Blob", ["moxie/runtime/html5/Runtime", "moxie/file/Blob"], function (n, t) {
            function i() {
                function n(n, t, i) {
                    var r;
                    if (window.File.prototype.slice) try {
                        return n.slice(), n.slice(t, i)
                    } catch (u) {
                        return n.slice(t, i - t)
                    } else return (r = window.File.prototype.webkitSlice || window.File.prototype.mozSlice) ? r.call(n, t, i) : null
                }
                this.slice = function () {
                    return new t(this.getRuntime()
                        .uid, n.apply(this, arguments))
                };
                this.destroy = function () {
                    this.getRuntime()
                        .getShim()
                        .removeInstance(this.uid)
                }
            }
            return n.Blob = i
        });
        i("moxie/core/utils/Events", ["moxie/core/utils/Basic"], function (n) {
            function u() {
                this.returnValue = !1
            }

            function f() {
                this.cancelBubble = !0
            }
            var i = {}
                , t = "moxie_" + n.guid()
                , e = function (r, e, o, s) {
                    var h, c;
                    e = e.toLowerCase();
                    r.addEventListener ? (h = o, r.addEventListener(e, h, !1)) : r.attachEvent && (h = function () {
                        var n = window.event;
                        n.target || (n.target = n.srcElement);
                        n.preventDefault = u;
                        n.stopPropagation = f;
                        o(n)
                    }, r.attachEvent("on" + e, h));
                    r[t] || (r[t] = n.guid());
                    i.hasOwnProperty(r[t]) || (i[r[t]] = {});
                    c = i[r[t]];
                    c.hasOwnProperty(e) || (c[e] = []);
                    c[e].push({
                        func: h
                        , orig: o
                        , key: s
                    })
                }
                , r = function (r, u, f) {
                    var e, s, o;
                    if (u = u.toLowerCase(), r[t] && i[r[t]] && i[r[t]][u]) e = i[r[t]][u];
                    else return;
                    for (o = e.length - 1; o >= 0; o--)
                        if ((e[o].orig === f || e[o].key === f) && (r.removeEventListener ? r.removeEventListener(u, e[o].func, !1) : r.detachEvent && r.detachEvent("on" + u, e[o].func), e[o].orig = null, e[o].func = null, e.splice(o, 1), f !== s)) break;
                    if (e.length || delete i[r[t]][u], n.isEmptyObj(i[r[t]])) {
                        delete i[r[t]];
                        try {
                            delete r[t]
                        } catch (h) {
                            r[t] = s
                        }
                    }
                }
                , o = function (u, f) {
                    u && u[t] && n.each(i[u[t]], function (n, t) {
                        r(u, t, f)
                    })
                };
            return {
                addEvent: e
                , removeEvent: r
                , removeAllEvents: o
            }
        });
        i("moxie/runtime/html5/file/FileInput", ["moxie/runtime/html5/Runtime", "moxie/file/File", "moxie/core/utils/Basic", "moxie/core/utils/Dom", "moxie/core/utils/Events", "moxie/core/utils/Mime", "moxie/core/utils/Env"], function (n, t, i, r, u, f, e) {
            function o() {
                var n, o;
                i.extend(this, {
                    init: function (s) {
                        var h = this
                            , c = h.getRuntime()
                            , a, v, p, l, w, y;
                        n = s;
                        p = f.extList2mimes(n.accept, c.can("filter_by_extension"));
                        v = c.getShimContainer();
                        v.innerHTML = '<input id="' + c.uid + '" type="file" style="font-size:999px;opacity:0;"' + (n.multiple && c.can("select_multiple") ? "multiple" : "") + (n.directory && c.can("select_folder") ? "webkitdirectory directory" : "") + (p ? ' accept="' + p.join(",") + '"' : "") + " />";
                        a = r.get(c.uid);
                        i.extend(a.style, {
                            position: "absolute"
                            , top: 0
                            , left: 0
                            , width: "100%"
                            , height: "100%"
                        });
                        l = r.get(n.browse_button);
                        o = r.getStyle(l, "z-index") || "auto";
                        c.can("summon_file_dialog") && (r.getStyle(l, "position") === "static" && (l.style.position = "relative"), u.addEvent(l, "click", function (n) {
                            var t = r.get(c.uid);
                            t && !t.disabled && t.click();
                            n.preventDefault()
                        }, h.uid), h.bind("Refresh", function () {
                            w = parseInt(o, 10) || 1;
                            r.get(n.browse_button)
                                .style.zIndex = w;
                            this.getRuntime()
                                .getShimContainer()
                                .style.zIndex = w - 1
                        }));
                        y = c.can("summon_file_dialog") ? l : v;
                        u.addEvent(y, "mouseover", function () {
                            h.trigger("mouseenter")
                        }, h.uid);
                        u.addEvent(y, "mouseout", function () {
                            h.trigger("mouseleave")
                        }, h.uid);
                        u.addEvent(y, "mousedown", function () {
                            h.trigger("mousedown")
                        }, h.uid);
                        u.addEvent(r.get(n.container), "mouseup", function () {
                            h.trigger("mouseup")
                        }, h.uid);
                        (c.can("summon_file_dialog") ? a : l)
                            .setAttribute("tabindex", -1);
                        a.onchange = function b() {
                            if (h.files = [], i.each(this.files, function (i) {
                                var r = "";
                                if (n.directory && i.name == ".") return !0;
                                i.webkitRelativePath && (r = "/" + i.webkitRelativePath.replace(/^\//, ""));
                                i = new t(c.uid, i);
                                i.relativePath = r;
                                h.files.push(i)
                            }), e.browser !== "IE" && e.browser !== "IEMobile") this.value = "";
                            else {
                                var r = this.cloneNode(!0);
                                this.parentNode.replaceChild(r, this);
                                r.onchange = b
                            }
                            h.files.length && h.trigger("change")
                        };
                        h.trigger({
                            type: "ready"
                            , async: !0
                        });
                        v = null
                    }
                    , setOption: function (n, t) {
                        var u = this.getRuntime()
                            , i = r.get(u.uid)
                            , e;
                        switch (n) {
                            case "accept":
                                t ? (e = t.mimes || f.extList2mimes(t, u.can("filter_by_extension")), i.setAttribute("accept", e.join(","))) : i.removeAttribute("accept");
                                break;
                            case "directory":
                                t && u.can("select_folder") ? (i.setAttribute("directory", ""), i.setAttribute("webkitdirectory", "")) : (i.removeAttribute("directory"), i.removeAttribute("webkitdirectory"));
                                break;
                            case "multiple":
                                t && u.can("select_multiple") ? i.setAttribute("multiple", "") : i.removeAttribute("multiple")
                        }
                    }
                    , disable: function (n) {
                        var i = this.getRuntime()
                            , t;
                        (t = r.get(i.uid)) && (t.disabled = !!n)
                    }
                    , destroy: function () {
                        var e = this.getRuntime()
                            , s = e.getShim()
                            , t = e.getShimContainer()
                            , f = n && r.get(n.container)
                            , i = n && r.get(n.browse_button);
                        f && u.removeAllEvents(f, this.uid);
                        i && (u.removeAllEvents(i, this.uid), i.style.zIndex = o);
                        t && (u.removeAllEvents(t, this.uid), t.innerHTML = "");
                        s.removeInstance(this.uid);
                        n = t = f = i = s = null
                    }
                })
            }
            return n.FileInput = o
        });
        i("moxie/runtime/html5/file/FileDrop", ["moxie/runtime/html5/Runtime", "moxie/file/File", "moxie/core/utils/Basic", "moxie/core/utils/Dom", "moxie/core/utils/Events", "moxie/core/utils/Mime"], function (n, t, i, r, u, f) {
            function e() {
                function c(n) {
                    if (!n.dataTransfer || !n.dataTransfer.types) return !1;
                    var t = i.toArray(n.dataTransfer.types || []);
                    return i.inArray("Files", t) !== -1 || i.inArray("public.file-url", t) !== -1 || i.inArray("application/x-moz-file", t) !== -1
                }

                function s(i, r) {
                    if (y(i)) {
                        var u = new t(h, i);
                        u.relativePath = r || "";
                        n.push(u)
                    }
                }

                function v(n) {
                    for (var t = [], r = 0; r < n.length; r++) [].push.apply(t, n[r].extensions.split(/\s*,\s*/));
                    return i.inArray("*", t) === -1 ? t : []
                }

                function y(n) {
                    if (!o.length) return !0;
                    var t = f.getFileExtension(n.name);
                    return !t || i.inArray(t, o) !== -1
                }

                function l(n, t) {
                    var r = [];
                    i.each(n, function (n) {
                        var t = n.webkitGetAsEntry();
                        t && (t.isFile ? s(n.getAsFile(), t.fullPath) : r.push(t))
                    });
                    r.length ? a(r, t) : t()
                }

                function a(n, t) {
                    var r = [];
                    i.each(n, function (n) {
                        r.push(function (t) {
                            p(n, t)
                        })
                    });
                    i.inSeries(r, function () {
                        t()
                    })
                }

                function p(n, t) {
                    n.isFile ? n.file(function (i) {
                        s(i, n.fullPath);
                        t()
                    }, function () {
                        t()
                    }) : n.isDirectory ? w(n, t) : t()
                }

                function w(n, t) {
                    function r(n) {
                        u.readEntries(function (t) {
                            t.length ? ([].push.apply(i, t), r(n)) : n()
                        }, n)
                    }
                    var i = []
                        , u = n.createReader();
                    r(function () {
                        a(i, t)
                    })
                }
                var n = []
                    , o = []
                    , e, h;
                i.extend(this, {
                    init: function (t) {
                        var r = this
                            , f;
                        e = t;
                        h = r.ruid;
                        o = v(e.accept);
                        f = e.container;
                        u.addEvent(f, "dragover", function (n) {
                            c(n) && (n.preventDefault(), n.dataTransfer.dropEffect = "copy")
                        }, r.uid);
                        u.addEvent(f, "drop", function (t) {
                            c(t) && (t.preventDefault(), n = [], t.dataTransfer.items && t.dataTransfer.items[0].webkitGetAsEntry ? l(t.dataTransfer.items, function () {
                                r.files = n;
                                r.trigger("drop")
                            }) : (i.each(t.dataTransfer.files, function (n) {
                                s(n)
                            }), r.files = n, r.trigger("drop")))
                        }, r.uid);
                        u.addEvent(f, "dragenter", function () {
                            r.trigger("dragenter")
                        }, r.uid);
                        u.addEvent(f, "dragleave", function () {
                            r.trigger("dragleave")
                        }, r.uid)
                    }
                    , getFromEvent: function (t, r) {
                        c(t) && (t.preventDefault(), n = [], t.dataTransfer.items && t.dataTransfer.items[0].webkitGetAsEntry ? l(t.dataTransfer.items, function () {
                            r(n)
                        }) : (i.each(t.dataTransfer.files, function (n) {
                            s(n)
                        }), r(n)))
                    }
                    , destroy: function () {
                        u.removeAllEvents(e && r.get(e.container), this.uid);
                        h = n = o = e = null;
                        this.getRuntime()
                            .getShim()
                            .removeInstance(this.uid)
                    }
                })
            }
            return window.Html5FileDrop = n.FileDrop = e
        });
        i("moxie/runtime/html5/file/FileReader", ["moxie/runtime/html5/Runtime", "moxie/core/utils/Encode", "moxie/core/utils/Basic"], function (n, t, i) {
            function r() {
                function u(n) {
                    return t.atob(n.substring(n.indexOf("base64,") + 7))
                }
                var n, r = !1;
                i.extend(this, {
                    read: function (t, f) {
                        var e = this;
                        e.result = "";
                        n = new window.FileReader;
                        n.addEventListener("progress", function (n) {
                            e.trigger(n)
                        });
                        n.addEventListener("load", function (t) {
                            e.result = r ? u(n.result) : n.result;
                            e.trigger(t)
                        });
                        n.addEventListener("error", function (t) {
                            e.trigger(t, n.error)
                        });
                        n.addEventListener("loadend", function (t) {
                            n = null;
                            e.trigger(t)
                        });
                        i.typeOf(n[t]) === "function" ? (r = !1, n[t](f.getSource())) : t === "readAsBinaryString" && (r = !0, n.readAsDataURL(f.getSource()))
                    }
                    , abort: function () {
                        n && n.abort()
                    }
                    , destroy: function () {
                        n = null;
                        this.getRuntime()
                            .getShim()
                            .removeInstance(this.uid)
                    }
                })
            }
            return n.FileReader = r
        });
        i("moxie/runtime/html5/xhr/XMLHttpRequest", ["moxie/runtime/html5/Runtime", "moxie/core/utils/Basic", "moxie/core/utils/Mime", "moxie/core/utils/Url", "moxie/file/File", "moxie/file/Blob", "moxie/xhr/FormData", "moxie/core/Exceptions", "moxie/core/utils/Env"], function (n, t, i, r, u, f, e, o, s) {
            function h() {
                function l(n, t) {
                    var u = this
                        , r, i;
                    r = t.getBlob()
                        .getSource();
                    i = new window.FileReader;
                    i.onload = function () {
                        t.append(t.getBlobName(), new f(null, {
                            type: r.type
                            , data: i.result
                        }));
                        c.send.call(u, n, t)
                    };
                    i.readAsBinaryString(r)
                }

                function a() {
                    return !window.XMLHttpRequest || s.browser === "IE" && s.verComp(s.version, 8, "<") ? function () {
                        for (var t = ["Msxml2.XMLHTTP.6.0", "Microsoft.XMLHTTP"], n = 0; n < t.length; n++) try {
                            return new ActiveXObject(t[n])
                        } catch (i) { }
                    }() : new window.XMLHttpRequest
                }

                function v(n) {
                    var t = n.responseXML
                        , i = n.responseText;
                    return (s.browser === "IE" && i && t && !t.documentElement && /[^\/]+\/[^\+]+\+xml/.test(n.getResponseHeader("Content-Type")) && (t = new window.ActiveXObject("Microsoft.XMLDOM"), t.async = !1, t.validateOnParse = !1, t.loadXML(i)), t && (s.browser === "IE" && t.parseError !== 0 || !t.documentElement || t.documentElement.tagName === "parsererror")) ? null : t
                }

                function y(t) {
                    var r = "----moxieboundary" + (new Date)
                        .getTime()
                        , u = "--"
                        , i = "\r\n"
                        , e = ""
                        , s = this.getRuntime();
                    if (!s.can("send_binary_string")) throw new o.RuntimeError(o.RuntimeError.NOT_SUPPORTED_ERR);
                    return n.setRequestHeader("Content-Type", "multipart/form-data; boundary=" + r), t.each(function (n, t) {
                        e += n instanceof f ? u + r + i + 'Content-Disposition: form-data; name="' + t + '"; filename="' + unescape(encodeURIComponent(n.name || "blob")) + '"' + i + "Content-Type: " + (n.type || "application/octet-stream") + i + i + n.getSource() + i : u + r + i + 'Content-Disposition: form-data; name="' + t + '"' + i + i + unescape(encodeURIComponent(n)) + i
                    }), e += u + r + u + i
                }
                var c = this
                    , n, h;
                t.extend(this, {
                    send: function (i, u) {
                        var o = this
                            , p = s.browser === "Mozilla" && s.verComp(s.version, 4, ">=") && s.verComp(s.version, 7, "<")
                            , w = s.browser === "Android Browser"
                            , v = !1
                            , c;
                        if (h = i.url.replace(/^.+?\/([\w\-\.]+)$/, "$1")
                            .toLowerCase(), n = a(), n.open(i.method, i.url, i.async, i.user, i.password), u instanceof f) u.isDetached() && (v = !0), u = u.getSource();
                        else if (u instanceof e) {
                            if (u.hasBlob())
                                if (u.getBlob()
                                    .isDetached()) u = y.call(o, u), v = !0;
                                else if ((p || w) && t.typeOf(u.getBlob()
                                    .getSource()) === "blob" && window.FileReader) {
                                    l.call(o, i, u);
                                    return
                                }
                            u instanceof e && (c = new window.FormData, u.each(function (n, t) {
                                n instanceof f ? c.append(t, n.getSource()) : c.append(t, n)
                            }), u = c)
                        }
                        n.upload ? (i.withCredentials && (n.withCredentials = !0), n.addEventListener("load", function (n) {
                            o.trigger(n)
                        }), n.addEventListener("error", function (n) {
                            o.trigger(n)
                        }), n.addEventListener("progress", function (n) {
                            o.trigger(n)
                        }), n.upload.addEventListener("progress", function (n) {
                            o.trigger({
                                type: "UploadProgress"
                                , loaded: n.loaded
                                , total: n.total
                            })
                        })) : n.onreadystatechange = function () {
                            switch (n.readyState) {
                                case 3:
                                    var t, u;
                                    try {
                                        r.hasSameOrigin(i.url) && (t = n.getResponseHeader("Content-Length") || 0);
                                        n.responseText && (u = n.responseText.length)
                                    } catch (f) {
                                        t = u = 0
                                    }
                                    o.trigger({
                                        type: "progress"
                                        , lengthComputable: !!t
                                        , total: parseInt(t, 10)
                                        , loaded: u
                                    });
                                    break;
                                case 4:
                                    n.onreadystatechange = function () { };
                                    try {
                                        if (n.status >= 200 && n.status < 400) {
                                            o.trigger("load");
                                            break
                                        }
                                    } catch (f) { }
                                    o.trigger("error")
                            }
                        };
                        t.isEmptyObj(i.headers) || t.each(i.headers, function (t, i) {
                            n.setRequestHeader(i, t)
                        });
                        "" !== i.responseType && "responseType" in n && (n.responseType = "json" !== i.responseType || s.can("return_response_type", "json") ? i.responseType : "text");
                        v ? n.sendAsBinary ? n.sendAsBinary(u) : function () {
                            for (var i = new Uint8Array(u.length), t = 0; t < u.length; t++) i[t] = u.charCodeAt(t) & 255;
                            n.send(i.buffer)
                        }() : n.send(u);
                        o.trigger("loadstart")
                    }
                    , getStatus: function () {
                        try {
                            if (n) return n.status
                        } catch (t) { }
                        return 0
                    }
                    , getResponse: function (t) {
                        var o = this.getRuntime()
                            , r, f, e;
                        try {
                            switch (t) {
                                case "blob":
                                    return r = new u(o.uid, n.response), f = n.getResponseHeader("Content-Disposition"), f && (e = f.match(/filename=([\'\"'])([^\1]+)\1/), e && (h = e[2])), r.name = h, r.type || (r.type = i.getFileMime(h)), r;
                                case "json":
                                    return s.can("return_response_type", "json") ? n.response : n.status === 200 && !!window.JSON ? JSON.parse(n.responseText) : null;
                                case "document":
                                    return v(n);
                                default:
                                    return n.responseText !== "" ? n.responseText : null
                            }
                        } catch (c) {
                            return null
                        }
                    }
                    , getAllResponseHeaders: function () {
                        try {
                            return n.getAllResponseHeaders()
                        } catch (t) { }
                        return ""
                    }
                    , abort: function () {
                        n && n.abort()
                    }
                    , destroy: function () {
                        c = h = null;
                        var n = this.getRuntime();
                        n && n.getShim()
                            .removeInstance(this.uid)
                    }
                })
            }
            return n.XMLHttpRequest = h
        });
        i("moxie/runtime/html5/utils/BinaryReader", ["moxie/core/utils/Basic"], function (n) {
            function t(n) {
                n instanceof ArrayBuffer ? i.apply(this, arguments) : r.apply(this, arguments)
            }

            function i(t) {
                var i = new DataView(t);
                n.extend(this, {
                    readByteAt: function (n) {
                        return i.getUint8(n)
                    }
                    , writeByteAt: function (n, t) {
                        i.setUint8(n, t)
                    }
                    , SEGMENT: function (n, r, u) {
                        switch (arguments.length) {
                            case 2:
                                return t.slice(n, n + r);
                            case 1:
                                return t.slice(n);
                            case 3:
                                if (u === null && (u = new ArrayBuffer), u instanceof ArrayBuffer) {
                                    var f = new Uint8Array(this.length() - r + u.byteLength);
                                    n > 0 && f.set(new Uint8Array(t.slice(0, n)), 0);
                                    f.set(new Uint8Array(u), n);
                                    f.set(new Uint8Array(t.slice(n + r)), n + u.byteLength);
                                    this.clear();
                                    t = f.buffer;
                                    i = new DataView(t);
                                    break
                                }
                            default:
                                return t
                        }
                    }
                    , length: function () {
                        return t ? t.byteLength : 0
                    }
                    , clear: function () {
                        i = t = null
                    }
                })
            }

            function r(t) {
                function i(n, i, r) {
                    r = arguments.length === 3 ? r : t.length - i - 1;
                    t = t.substr(0, i) + n + t.substr(r + i)
                }
                n.extend(this, {
                    readByteAt: function (n) {
                        return t.charCodeAt(n)
                    }
                    , writeByteAt: function (n, t) {
                        i(String.fromCharCode(t), n, 1)
                    }
                    , SEGMENT: function (n, r, u) {
                        switch (arguments.length) {
                            case 1:
                                return t.substr(n);
                            case 2:
                                return t.substr(n, r);
                            case 3:
                                i(u !== null ? u : "", n, r);
                                break;
                            default:
                                return t
                        }
                    }
                    , length: function () {
                        return t ? t.length : 0
                    }
                    , clear: function () {
                        t = null
                    }
                })
            }
            return n.extend(t.prototype, {
                littleEndian: !1
                , read: function (n, t) {
                    var r, u, i;
                    if (n + t > this.length()) throw new Error("You are trying to read outside the source boundaries.");
                    for (u = this.littleEndian ? 0 : -8 * (t - 1), i = 0, r = 0; i < t; i++) r |= this.readByteAt(n + i) << Math.abs(u + i * 8);
                    return r
                }
                , write: function (n, t, i) {
                    var u, r;
                    if (n > this.length()) throw new Error("You are trying to write outside the source boundaries.");
                    for (u = this.littleEndian ? 0 : -8 * (i - 1), r = 0; r < i; r++) this.writeByteAt(n + r, t >> Math.abs(u + r * 8) & 255)
                }
                , BYTE: function (n) {
                    return this.read(n, 1)
                }
                , SHORT: function (n) {
                    return this.read(n, 2)
                }
                , LONG: function (n) {
                    return this.read(n, 4)
                }
                , SLONG: function (n) {
                    var t = this.read(n, 4);
                    return t > 2147483647 ? t - 4294967296 : t
                }
                , CHAR: function (n) {
                    return String.fromCharCode(this.read(n, 1))
                }
                , STRING: function (n, t) {
                    return this.asArray("CHAR", n, t)
                        .join("")
                }
                , asArray: function (n, t, i) {
                    for (var u = [], r = 0; r < i; r++) u[r] = this[n](t + r);
                    return u
                }
            }), t
        });
        i("moxie/runtime/html5/image/JPEGHeaders", ["moxie/runtime/html5/utils/BinaryReader", "moxie/core/Exceptions"], function (n, t) {
            return function i(r) {
                var u = []
                    , o, f, e, s = 0;
                if (o = new n(r), o.SHORT(0) !== 65496) {
                    o.clear();
                    throw new t.ImageError(t.ImageError.WRONG_FORMAT);
                }
                for (f = 2; f <= o.length();) {
                    if (e = o.SHORT(f), e >= 65488 && e <= 65495) {
                        f += 2;
                        continue
                    }
                    if (e === 65498 || e === 65497) break;
                    s = o.SHORT(f + 2) + 2;
                    e >= 65505 && e <= 65519 && u.push({
                        hex: e
                        , name: "APP" + (e & 15)
                        , start: f
                        , length: s
                        , segment: o.SEGMENT(f, s)
                    });
                    f += s
                }
                return o.clear(), {
                    headers: u
                    , restore: function (t) {
                        var e, r, i;
                        for (i = new n(t), f = i.SHORT(2) == 65504 ? 4 + i.SHORT(4) : 2, r = 0, e = u.length; r < e; r++) i.SEGMENT(f, 0, u[r].segment), f += u[r].length;
                        return t = i.SEGMENT(), i.clear(), t
                    }
                    , strip: function (t) {
                        var r, u, e, f;
                        for (e = new i(t), u = e.headers, e.purge(), r = new n(t), f = u.length; f--;) r.SEGMENT(u[f].start, u[f].length, "");
                        return t = r.SEGMENT(), r.clear(), t
                    }
                    , get: function (n) {
                        for (var i = [], t = 0, r = u.length; t < r; t++) u[t].name === n.toUpperCase() && i.push(u[t].segment);
                        return i
                    }
                    , set: function (n, t) {
                        var r = []
                            , i, f, e;
                        for (typeof t == "string" ? r.push(t) : r = t, i = f = 0, e = u.length; i < e; i++)
                            if (u[i].name === n.toUpperCase() && (u[i].segment = r[f], u[i].length = r[f].length, f++), f >= r.length) break
                    }
                    , purge: function () {
                        this.headers = u = []
                    }
                }
            }
        });
        i("moxie/runtime/html5/image/ExifParser", ["moxie/core/utils/Basic", "moxie/runtime/html5/utils/BinaryReader", "moxie/core/Exceptions"], function (n, i, r) {
            function u(f) {
                function l(i, u) {
                    for (var o = this, s, a, c, v, f, y, p = [], w = {}, k = {
                        1: "BYTE"
                        , 7: "UNDEFINED"
                        , 2: "ASCII"
                        , 3: "SHORT"
                        , 4: "LONG"
                        , 5: "RATIONAL"
                        , 9: "SLONG"
                        , 10: "SRATIONAL"
                    }, d = {
                        BYTE: 1
                        , UNDEFINED: 1
                        , ASCII: 1
                        , SHORT: 2
                        , LONG: 4
                        , RATIONAL: 8
                        , SLONG: 4
                        , SRATIONAL: 8
                    }, b = o.SHORT(i), l = 0; l < b; l++)
                        if (p = [], f = i + 2 + l * 12, s = u[o.SHORT(f)], s !== t) {
                            if (a = k[o.SHORT(f += 2)], c = o.LONG(f += 2), v = d[a], !v) throw new r.ImageError(r.ImageError.INVALID_META_ERR);
                            if (f += 4, v * c > 4 && (f = o.LONG(f) + e.tiffHeader), f + v * c >= this.length()) throw new r.ImageError(r.ImageError.INVALID_META_ERR);
                            if (a === "ASCII") {
                                w[s] = n.trim(o.STRING(f, c)
                                    .replace(/\0$/, ""));
                                continue
                            } else p = o.asArray(a, f, c), y = c == 1 ? p[0] : p, w[s] = h.hasOwnProperty(s) && typeof y != "object" ? h[s][y] : y
                        } return w
                }

                function y(n, t, i) {
                    var u, l, f, o = 0
                        , h, c, r;
                    if (typeof t == "string") {
                        h = s[n.toLowerCase()];
                        for (c in h)
                            if (h[c] === t) {
                                t = c;
                                break
                            }
                    }
                    for (u = e[n.toLowerCase() + "IFD"], l = this.SHORT(u), r = 0; r < l; r++)
                        if (f = u + 12 * r + 2, this.SHORT(f) == t) {
                            o = f + 8;
                            break
                        } if (!o) return !1;
                    try {
                        this.write(o, i, 4)
                    } catch (a) {
                        return !1
                    }
                    return !0
                }
                var a, s, h, e, c, o, v;
                if (i.call(this, f), s = {
                    tiff: {
                        274: "Orientation"
                        , 270: "ImageDescription"
                        , 271: "Make"
                        , 272: "Model"
                        , 305: "Software"
                        , 34665: "ExifIFDPointer"
                        , 34853: "GPSInfoIFDPointer"
                    }
                    , exif: {
                        36864: "ExifVersion"
                        , 40961: "ColorSpace"
                        , 40962: "PixelXDimension"
                        , 40963: "PixelYDimension"
                        , 36867: "DateTimeOriginal"
                        , 33434: "ExposureTime"
                        , 33437: "FNumber"
                        , 34855: "ISOSpeedRatings"
                        , 37377: "ShutterSpeedValue"
                        , 37378: "ApertureValue"
                        , 37383: "MeteringMode"
                        , 37384: "LightSource"
                        , 37385: "Flash"
                        , 37386: "FocalLength"
                        , 41986: "ExposureMode"
                        , 41987: "WhiteBalance"
                        , 41990: "SceneCaptureType"
                        , 41988: "DigitalZoomRatio"
                        , 41992: "Contrast"
                        , 41993: "Saturation"
                        , 41994: "Sharpness"
                    }
                    , gps: {
                        0: "GPSVersionID"
                        , 1: "GPSLatitudeRef"
                        , 2: "GPSLatitude"
                        , 3: "GPSLongitudeRef"
                        , 4: "GPSLongitude"
                    }
                    , thumb: {
                        513: "JPEGInterchangeFormat"
                        , 514: "JPEGInterchangeFormatLength"
                    }
                }, h = {
                    ColorSpace: {
                        1: "sRGB"
                        , 0: "Uncalibrated"
                    }
                    , MeteringMode: {
                        0: "Unknown"
                        , 1: "Average"
                        , 2: "CenterWeightedAverage"
                        , 3: "Spot"
                        , 4: "MultiSpot"
                        , 5: "Pattern"
                        , 6: "Partial"
                        , 255: "Other"
                    }
                    , LightSource: {
                        1: "Daylight"
                        , 2: "Fliorescent"
                        , 3: "Tungsten"
                        , 4: "Flash"
                        , 9: "Fine weather"
                        , 10: "Cloudy weather"
                        , 11: "Shade"
                        , 12: "Daylight fluorescent (D 5700 - 7100K)"
                        , 13: "Day white fluorescent (N 4600 -5400K)"
                        , 14: "Cool white fluorescent (W 3900 - 4500K)"
                        , 15: "White fluorescent (WW 3200 - 3700K)"
                        , 17: "Standard light A"
                        , 18: "Standard light B"
                        , 19: "Standard light C"
                        , 20: "D55"
                        , 21: "D65"
                        , 22: "D75"
                        , 23: "D50"
                        , 24: "ISO studio tungsten"
                        , 255: "Other"
                    }
                    , Flash: {
                        0: "Flash did not fire"
                        , 1: "Flash fired"
                        , 5: "Strobe return light not detected"
                        , 7: "Strobe return light detected"
                        , 9: "Flash fired, compulsory flash mode"
                        , 13: "Flash fired, compulsory flash mode, return light not detected"
                        , 15: "Flash fired, compulsory flash mode, return light detected"
                        , 16: "Flash did not fire, compulsory flash mode"
                        , 24: "Flash did not fire, auto mode"
                        , 25: "Flash fired, auto mode"
                        , 29: "Flash fired, auto mode, return light not detected"
                        , 31: "Flash fired, auto mode, return light detected"
                        , 32: "No flash function"
                        , 65: "Flash fired, red-eye reduction mode"
                        , 69: "Flash fired, red-eye reduction mode, return light not detected"
                        , 71: "Flash fired, red-eye reduction mode, return light detected"
                        , 73: "Flash fired, compulsory flash mode, red-eye reduction mode"
                        , 77: "Flash fired, compulsory flash mode, red-eye reduction mode, return light not detected"
                        , 79: "Flash fired, compulsory flash mode, red-eye reduction mode, return light detected"
                        , 89: "Flash fired, auto mode, red-eye reduction mode"
                        , 93: "Flash fired, auto mode, return light not detected, red-eye reduction mode"
                        , 95: "Flash fired, auto mode, return light detected, red-eye reduction mode"
                    }
                    , ExposureMode: {
                        0: "Auto exposure"
                        , 1: "Manual exposure"
                        , 2: "Auto bracket"
                    }
                    , WhiteBalance: {
                        0: "Auto white balance"
                        , 1: "Manual white balance"
                    }
                    , SceneCaptureType: {
                        0: "Standard"
                        , 1: "Landscape"
                        , 2: "Portrait"
                        , 3: "Night scene"
                    }
                    , Contrast: {
                        0: "Normal"
                        , 1: "Soft"
                        , 2: "Hard"
                    }
                    , Saturation: {
                        0: "Normal"
                        , 1: "Low saturation"
                        , 2: "High saturation"
                    }
                    , Sharpness: {
                        0: "Normal"
                        , 1: "Soft"
                        , 2: "Hard"
                    }
                    , GPSLatitudeRef: {
                        N: "North latitude"
                        , S: "South latitude"
                    }
                    , GPSLongitudeRef: {
                        E: "East longitude"
                        , W: "West longitude"
                    }
                }, e = {
                    tiffHeader: 10
                }, c = e.tiffHeader, a = {
                    clear: this.clear
                }, n.extend(this, {
                    read: function () {
                        try {
                            return u.prototype.read.apply(this, arguments)
                        } catch (n) {
                            throw new r.ImageError(r.ImageError.INVALID_META_ERR);
                        }
                    }
                    , write: function () {
                        try {
                            return u.prototype.write.apply(this, arguments)
                        } catch (n) {
                            throw new r.ImageError(r.ImageError.INVALID_META_ERR);
                        }
                    }
                    , UNDEFINED: function () {
                        return this.BYTE.apply(this, arguments)
                    }
                    , RATIONAL: function (n) {
                        return this.LONG(n) / this.LONG(n + 4)
                    }
                    , SRATIONAL: function (n) {
                        return this.SLONG(n) / this.SLONG(n + 4)
                    }
                    , ASCII: function (n) {
                        return this.CHAR(n)
                    }
                    , TIFF: function () {
                        return o || null
                    }
                    , EXIF: function () {
                        var t = null
                            , i, r;
                        if (e.exifIFD) {
                            try {
                                t = l.call(this, e.exifIFD, s.exif)
                            } catch (u) {
                                return null
                            }
                            if (t.ExifVersion && n.typeOf(t.ExifVersion) === "array") {
                                for (i = 0, r = ""; i < t.ExifVersion.length; i++) r += String.fromCharCode(t.ExifVersion[i]);
                                t.ExifVersion = r
                            }
                        }
                        return t
                    }
                    , GPS: function () {
                        var t = null;
                        if (e.gpsIFD) {
                            try {
                                t = l.call(this, e.gpsIFD, s.gps)
                            } catch (i) {
                                return null
                            }
                            t.GPSVersionID && n.typeOf(t.GPSVersionID) === "array" && (t.GPSVersionID = t.GPSVersionID.join("."))
                        }
                        return t
                    }
                    , thumb: function () {
                        if (e.IFD1) try {
                            var n = l.call(this, e.IFD1, s.thumb);
                            if ("JPEGInterchangeFormat" in n) return this.SEGMENT(e.tiffHeader + n.JPEGInterchangeFormat, n.JPEGInterchangeFormatLength)
                        } catch (t) { }
                        return null
                    }
                    , setExif: function (n, t) {
                        return n !== "PixelXDimension" && n !== "PixelYDimension" ? !1 : y.call(this, "exif", n, t)
                    }
                    , clear: function () {
                        a.clear();
                        f = s = h = o = e = a = null
                    }
                }), this.SHORT(0) !== 65505 || this.STRING(4, 5)
                    .toUpperCase() !== "EXIF\0") throw new r.ImageError(r.ImageError.INVALID_META_ERR);
                if (this.littleEndian = this.SHORT(c) == 18761, this.SHORT(c += 2) !== 42) throw new r.ImageError(r.ImageError.INVALID_META_ERR);
                e.IFD0 = e.tiffHeader + this.LONG(c += 2);
                o = l.call(this, e.IFD0, s.tiff);
                "ExifIFDPointer" in o && (e.exifIFD = e.tiffHeader + o.ExifIFDPointer, delete o.ExifIFDPointer);
                "GPSInfoIFDPointer" in o && (e.gpsIFD = e.tiffHeader + o.GPSInfoIFDPointer, delete o.GPSInfoIFDPointer);
                n.isEmptyObj(o) && (o = null);
                v = this.LONG(e.IFD0 + this.SHORT(e.IFD0) * 12 + 2);
                v && (e.IFD1 = e.tiffHeader + v)
            }
            return u.prototype = i.prototype, u
        });
        i("moxie/runtime/html5/image/JPEG", ["moxie/core/utils/Basic", "moxie/core/Exceptions", "moxie/runtime/html5/image/JPEGHeaders", "moxie/runtime/html5/utils/BinaryReader", "moxie/runtime/html5/image/ExifParser"], function (n, t, i, r, u) {
            function f(f) {
                function c(n) {
                    var t = 0
                        , i, r;
                    for (n || (n = s); t <= n.length();) {
                        if (i = n.SHORT(t += 2), i >= 65472 && i <= 65475) return t += 5, {
                            height: n.SHORT(t)
                            , width: n.SHORT(t += 2)
                        };
                        r = n.SHORT(t += 2);
                        t += r - 2
                    }
                    return null
                }

                function l() {
                    var t = e.thumb()
                        , i, n;
                    return t && (i = new r(t), n = c(i), i.clear(), n) ? (n.data = t, n) : null
                }

                function a() {
                    e && o && s && (e.clear(), o.purge(), s.clear(), h = o = e = s = null)
                }
                var s, o, e, h;
                if (s = new r(f), s.SHORT(0) !== 65496) throw new t.ImageError(t.ImageError.WRONG_FORMAT);
                o = new i(f);
                try {
                    e = new u(o.get("app1")[0])
                } catch (v) { }
                h = c.call(this);
                n.extend(this, {
                    type: "image/jpeg"
                    , size: s.length()
                    , width: h && h.width || 0
                    , height: h && h.height || 0
                    , setExif: function (t, i) {
                        if (!e) return !1;
                        n.typeOf(t) === "object" ? n.each(t, function (n, t) {
                            e.setExif(t, n)
                        }) : e.setExif(t, i);
                        o.set("app1", e.SEGMENT())
                    }
                    , writeHeaders: function () {
                        return arguments.length ? o.restore(arguments[0]) : o.restore(f)
                    }
                    , stripHeaders: function (n) {
                        return o.strip(n)
                    }
                    , purge: function () {
                        a.call(this)
                    }
                });
                e && (this.meta = {
                    tiff: e.TIFF()
                    , exif: e.EXIF()
                    , gps: e.GPS()
                    , thumb: l()
                })
            }
            return f
        });
        i("moxie/runtime/html5/image/PNG", ["moxie/core/Exceptions", "moxie/core/utils/Basic", "moxie/runtime/html5/utils/BinaryReader"], function (n, t, i) {
            function r(r) {
                function h() {
                    var n, t;
                    return (n = c.call(this, 8), n.type == "IHDR") ? (t = n.start, {
                        width: u.LONG(t)
                        , height: u.LONG(t += 4)
                    }) : null
                }

                function e() {
                    u && (u.clear(), r = f = o = s = u = null)
                }

                function c(n) {
                    var t, i, r, f;
                    return t = u.LONG(n), i = u.STRING(n += 4, 4), r = n += 4, f = u.LONG(n + t), {
                        length: t
                        , type: i
                        , start: r
                        , CRC: f
                    }
                }
                var u, o, s, f;
                u = new i(r)
                    , function () {
                        for (var i = 0, t = 0, r = [35152, 20039, 3338, 6666], t = 0; t < r.length; t++ , i += 2)
                            if (r[t] != u.SHORT(i)) throw new n.ImageError(n.ImageError.WRONG_FORMAT);
                    }();
                f = h.call(this);
                t.extend(this, {
                    type: "image/png"
                    , size: u.length()
                    , width: f.width
                    , height: f.height
                    , purge: function () {
                        e.call(this)
                    }
                });
                e.call(this)
            }
            return r
        });
        i("moxie/runtime/html5/image/ImageInfo", ["moxie/core/utils/Basic", "moxie/core/Exceptions", "moxie/runtime/html5/image/JPEG", "moxie/runtime/html5/image/PNG"], function (n, t, i, r) {
            return function (u) {
                var e = [i, r]
                    , f;
                f = function () {
                    for (var n = 0; n < e.length; n++) try {
                        return new e[n](u)
                    } catch (i) { }
                    throw new t.ImageError(t.ImageError.WRONG_FORMAT);
                }();
                n.extend(this, {
                    type: ""
                    , size: 0
                    , width: 0
                    , height: 0
                    , setExif: function () { }
                    , writeHeaders: function (n) {
                        return n
                    }
                    , stripHeaders: function (n) {
                        return n
                    }
                    , purge: function () {
                        u = null
                    }
                });
                n.extend(this, f);
                this.purge = function () {
                    f.purge();
                    f = null
                }
            }
        });
        i("moxie/runtime/html5/image/ResizerCanvas", [], function () {
            function n(i, r, u) {
                var e = i.width > i.height ? "width" : "height"
                    , s = Math.round(i[e] * r)
                    , o = !1
                    , f;
                return u !== "nearest" && (r < .5 || r > 2) && (r = r < .5 ? .5 : 2, o = !0), f = t(i, r), o ? n(f, s / f[e], u) : f
            }

            function t(n, t) {
                var r = n.width
                    , u = n.height
                    , f = Math.round(r * t)
                    , e = Math.round(u * t)
                    , i = document.createElement("canvas");
                return i.width = f, i.height = e, i.getContext("2d")
                    .drawImage(n, 0, 0, r, u, 0, 0, f, e), n = null, i
            }
            return {
                scale: n
            }
        });
        i("moxie/runtime/html5/image/Image", ["moxie/runtime/html5/Runtime", "moxie/core/utils/Basic", "moxie/core/Exceptions", "moxie/core/utils/Encode", "moxie/file/Blob", "moxie/file/File", "moxie/runtime/html5/image/ImageInfo", "moxie/runtime/html5/image/ResizerCanvas", "moxie/core/utils/Mime", "moxie/core/utils/Env"], function (n, t, i, r, u, f, e, o, s) {
            function h() {
                function w() {
                    if (!n && !a) throw new i.ImageError(i.DOMException.INVALID_STATE_ERR);
                    return n || a
                }

                function k() {
                    var t = w();
                    return t.nodeName.toLowerCase() == "canvas" ? t : (n = document.createElement("canvas"), n.width = t.width, n.height = t.height, n.getContext("2d")
                        .drawImage(t, 0, 0), n)
                }

                function b(n) {
                    return r.atob(n.substring(n.indexOf("base64,") + 7))
                }

                function nt(n, t) {
                    return "data:" + (t || "") + ";base64," + r.btoa(n)
                }

                function d(n) {
                    var t = this;
                    a = new Image;
                    a.onerror = function () {
                        g.call(this);
                        t.trigger("error", i.ImageError.WRONG_FORMAT)
                    };
                    a.onload = function () {
                        t.trigger("load")
                    };
                    a.src = n.substr(0, 5) == "data:" ? n : nt(n, l.type)
                }

                function tt(n, t) {
                    var u = this
                        , r;
                    if (window.FileReader) r = new FileReader, r.onload = function () {
                        t.call(u, this.result)
                    }, r.onerror = function () {
                        u.trigger("error", i.ImageError.WRONG_FORMAT)
                    }, r.readAsDataURL(n);
                    else return t.call(this, n.getAsDataURL())
                }

                function it(n, i) {
                    var o = Math.PI / 180
                        , e = document.createElement("canvas")
                        , r = e.getContext("2d")
                        , u = n.width
                        , f = n.height;
                    t.inArray(i, [5, 6, 7, 8]) > -1 ? (e.width = f, e.height = u) : (e.width = u, e.height = f);
                    switch (i) {
                        case 2:
                            r.translate(u, 0);
                            r.scale(-1, 1);
                            break;
                        case 3:
                            r.translate(u, f);
                            r.rotate(180 * o);
                            break;
                        case 4:
                            r.translate(0, f);
                            r.scale(1, -1);
                            break;
                        case 5:
                            r.rotate(90 * o);
                            r.scale(1, -1);
                            break;
                        case 6:
                            r.rotate(90 * o);
                            r.translate(0, -f);
                            break;
                        case 7:
                            r.rotate(90 * o);
                            r.translate(u, -f);
                            r.scale(-1, 1);
                            break;
                        case 8:
                            r.rotate(-90 * o);
                            r.translate(-u, 0)
                    }
                    return r.drawImage(n, 0, 0, u, f), e
                }

                function g() {
                    c && (c.purge(), c = null);
                    h = a = n = l = null;
                    v = !1
                }
                var y = this
                    , a, c, n, h, l, v = !1
                    , p = !0;
                t.extend(this, {
                    loadFromBlob: function (n) {
                        var t = this.getRuntime()
                            , r = arguments.length > 1 ? arguments[1] : !0;
                        if (!t.can("access_binary")) throw new i.RuntimeError(i.RuntimeError.NOT_SUPPORTED_ERR);
                        if (l = n, n.isDetached()) {
                            h = n.getSource();
                            d.call(this, h);
                            return
                        }
                        tt.call(this, n.getSource(), function (n) {
                            r && (h = b(n));
                            d.call(this, n)
                        })
                    }
                    , loadFromImage: function (n, t) {
                        this.meta = n.meta;
                        l = new f(null, {
                            name: n.name
                            , size: n.size
                            , type: n.type
                        });
                        d.call(this, t ? h = n.getAsBinaryString() : n.getAsDataURL())
                    }
                    , getInfo: function () {
                        var t = this.getRuntime()
                            , n;
                        return !c && h && t.can("access_image_binary") && (c = new e(h)), n = {
                            width: w()
                                .width || 0
                            , height: w()
                                .height || 0
                            , type: l.type || s.getFileMime(l.name)
                            , size: h && h.length || l.size || 0
                            , name: l.name || ""
                            , meta: null
                        }, p && (n.meta = c && c.meta || this.meta || {}, !n.meta || !n.meta.thumb || n.meta.thumb.data instanceof u || (n.meta.thumb.data = new u(null, {
                            type: "image/jpeg"
                            , data: n.meta.thumb.data
                        }))), n
                    }
                    , resize: function (t, i, r) {
                        var u = document.createElement("canvas")
                            , f;
                        u.width = t.width;
                        u.height = t.height;
                        u.getContext("2d")
                            .drawImage(w(), t.x, t.y, t.width, t.height, 0, 0, u.width, u.height);
                        n = o.scale(u, i);
                        p = r.preserveHeaders;
                        p || (f = this.meta && this.meta.tiff && this.meta.tiff.Orientation || 1, n = it(n, f));
                        this.width = n.width;
                        this.height = n.height;
                        v = !0;
                        this.trigger("Resize")
                    }
                    , getAsCanvas: function () {
                        return n || (n = k()), n.id = this.uid + "_canvas", n
                    }
                    , getAsBlob: function (n, t) {
                        return n !== this.type ? (v = !0, new f(null, {
                            name: l.name || ""
                            , type: n
                            , data: y.getAsDataURL(n, t)
                        })) : new f(null, {
                            name: l.name || ""
                            , type: n
                            , data: y.getAsBinaryString(n, t)
                        })
                    }
                    , getAsDataURL: function (t) {
                        var i = arguments[1] || 90;
                        if (!v) return a.src;
                        if (k(), "image/jpeg" !== t) return n.toDataURL("image/png");
                        try {
                            return n.toDataURL("image/jpeg", i / 100)
                        } catch (r) {
                            return n.toDataURL("image/jpeg")
                        }
                    }
                    , getAsBinaryString: function (t, i) {
                        if (!v) return h || (h = b(y.getAsDataURL(t, i))), h;
                        if ("image/jpeg" !== t) h = b(y.getAsDataURL(t, i));
                        else {
                            var r;
                            i || (i = 90);
                            k();
                            try {
                                r = n.toDataURL("image/jpeg", i / 100)
                            } catch (u) {
                                r = n.toDataURL("image/jpeg")
                            }
                            h = b(r);
                            c && (h = c.stripHeaders(h), p && (c.meta && c.meta.exif && c.setExif({
                                PixelXDimension: this.width
                                , PixelYDimension: this.height
                            }), h = c.writeHeaders(h)), c.purge(), c = null)
                        }
                        return v = !1, h
                    }
                    , destroy: function () {
                        y = null;
                        g.call(this);
                        this.getRuntime()
                            .getShim()
                            .removeInstance(this.uid)
                    }
                })
            }
            return n.Image = h
        });
        i("moxie/runtime/flash/Runtime", ["moxie/core/utils/Basic", "moxie/core/utils/Env", "moxie/core/utils/Dom", "moxie/core/Exceptions", "moxie/runtime/Runtime"], function (n, t, i, r, f) {
            function s() {
                var n;
                try {
                    n = navigator.plugins["Shockwave Flash"];
                    n = n.description
                } catch (t) {
                    try {
                        n = new ActiveXObject("ShockwaveFlash.ShockwaveFlash")
                            .GetVariable("$version")
                    } catch (i) {
                        n = "0.0"
                    }
                }
                return n = n.match(/\d+/g), parseFloat(n[0] + "." + n[1])
            }

            function h(n) {
                var r = i.get(n);
                r && r.nodeName == "OBJECT" && (t.browser === "IE" ? (r.style.display = "none", function u() {
                    r.readyState == 4 ? c(n) : setTimeout(u, 10)
                }()) : r.parentNode.removeChild(r))
            }

            function c(n) {
                var t = i.get(n)
                    , r;
                if (t) {
                    for (r in t) typeof t[r] == "function" && (t[r] = null);
                    t.parentNode.removeChild(t)
                }
            }

            function l(c) {
                var l = this
                    , a;
                c = n.extend({
                    swf_url: t.swf_url
                }, c);
                f.call(this, c, e, {
                    access_binary: function (n) {
                        return n && l.mode === "browser"
                    }
                    , access_image_binary: function (n) {
                        return n && l.mode === "browser"
                    }
                    , display_media: f.capTest(u("moxie/image/Image"))
                    , do_cors: f.capTrue
                    , drag_and_drop: !1
                    , report_upload_progress: function () {
                        return l.mode === "client"
                    }
                    , resize_image: f.capTrue
                    , return_response_headers: !1
                    , return_response_type: function (t) {
                        return t === "json" && !!window.JSON ? !0 : !n.arrayDiff(t, ["", "text", "document"]) || l.mode === "browser"
                    }
                    , return_status_code: function (t) {
                        return l.mode === "browser" || !n.arrayDiff(t, [200, 404])
                    }
                    , select_file: f.capTrue
                    , select_multiple: f.capTrue
                    , send_binary_string: function (n) {
                        return n && l.mode === "browser"
                    }
                    , send_browser_cookies: function (n) {
                        return n && l.mode === "browser"
                    }
                    , send_custom_headers: function (n) {
                        return n && l.mode === "browser"
                    }
                    , send_multipart: f.capTrue
                    , slice_blob: function (n) {
                        return n && l.mode === "browser"
                    }
                    , stream_upload: function (n) {
                        return n && l.mode === "browser"
                    }
                    , summon_file_dialog: !1
                    , upload_filesize: function (t) {
                        return n.parseSizeStr(t) <= 2097152 || l.mode === "client"
                    }
                    , use_http_method: function (t) {
                        return !n.arrayDiff(t, ["GET", "POST"])
                    }
                }, {
                    access_binary: function (n) {
                        return n ? "browser" : "client"
                    }
                    , access_image_binary: function (n) {
                        return n ? "browser" : "client"
                    }
                    , report_upload_progress: function (n) {
                        return n ? "browser" : "client"
                    }
                    , return_response_type: function (t) {
                        return n.arrayDiff(t, ["", "text", "json", "document"]) ? "browser" : ["client", "browser"]
                    }
                    , return_status_code: function (t) {
                        return n.arrayDiff(t, [200, 404]) ? "browser" : ["client", "browser"]
                    }
                    , send_binary_string: function (n) {
                        return n ? "browser" : "client"
                    }
                    , send_browser_cookies: function (n) {
                        return n ? "browser" : "client"
                    }
                    , send_custom_headers: function (n) {
                        return n ? "browser" : "client"
                    }
                    , slice_blob: function (n) {
                        return n ? "browser" : "client"
                    }
                    , stream_upload: function (n) {
                        return n ? "client" : "browser"
                    }
                    , upload_filesize: function (t) {
                        return n.parseSizeStr(t) >= 2097152 ? "client" : "browser"
                    }
                }, "client");
                s() < 11.3 && (MXI_DEBUG && t.debug.runtime && t.log("\tFlash didn't meet minimal version requirement (11.3)."), this.mode = !1);
                n.extend(this, {
                    getShim: function () {
                        return i.get(this.uid)
                    }
                    , shimExec: function (n, t) {
                        var i = [].slice.call(arguments, 2);
                        return l.getShim()
                            .exec(this.uid, n, t, i)
                    }
                    , init: function () {
                        var i, e, u;
                        u = this.getShimContainer();
                        n.extend(u.style, {
                            position: "absolute"
                            , top: "-8px"
                            , left: "-8px"
                            , width: "9px"
                            , height: "9px"
                            , overflow: "hidden"
                        });
                        i = '<object id="' + this.uid + '" type="application/x-shockwave-flash" data="' + c.swf_url + '" ';
                        t.browser === "IE" && (i += 'classid="clsid:d27cdb6e-ae6d-11cf-96b8-444553540000" ');
                        i += 'width="100%" height="100%" style="outline:0"><param name="movie" value="' + c.swf_url + '" /><param name="flashvars" value="uid=' + escape(this.uid) + "&target=" + f.getGlobalEventTarget() + '" /><param name="wmode" value="transparent" /><param name="allowscriptaccess" value="always" /><\/object>';
                        t.browser === "IE" ? (e = document.createElement("div"), u.appendChild(e), e.outerHTML = i, e = u = null) : u.innerHTML = i;
                        a = setTimeout(function () {
                            l && !l.initialized && (l.trigger("Error", new r.RuntimeError(r.RuntimeError.NOT_INIT_ERR)), MXI_DEBUG && t.debug.runtime && t.log("\tFlash failed to initialize within a specified period of time (typically 5s)."))
                        }, 5e3)
                    }
                    , destroy: function (n) {
                        return function () {
                            h(l.uid);
                            n.call(l);
                            clearTimeout(a);
                            c = a = n = l = null
                        }
                    }(this.destroy)
                }, o)
            }
            var e = "flash"
                , o = {};
            return f.addConstructor(e, l), o
        });
        i("moxie/runtime/flash/file/Blob", ["moxie/runtime/flash/Runtime", "moxie/file/Blob"], function (n, t) {
            var i = {
                slice: function (n, i, r, u) {
                    var f = this.getRuntime();
                    return i < 0 ? i = Math.max(n.size + i, 0) : i > 0 && (i = Math.min(i, n.size)), r < 0 ? r = Math.max(n.size + r, 0) : r > 0 && (r = Math.min(r, n.size)), n = f.shimExec.call(this, "Blob", "slice", i, r, u || ""), n && (n = new t(f.uid, n)), n
                }
            };
            return n.Blob = i
        });
        i("moxie/runtime/flash/file/FileInput", ["moxie/runtime/flash/Runtime", "moxie/file/File", "moxie/core/utils/Dom", "moxie/core/utils/Basic"], function (n, t, i, r) {
            var u = {
                init: function (n) {
                    var u = this
                        , e = this.getRuntime()
                        , f = i.get(n.browse_button);
                    f && (f.setAttribute("tabindex", -1), f = null);
                    this.bind("Change", function () {
                        var n = e.shimExec.call(u, "FileInput", "getFiles");
                        u.files = [];
                        r.each(n, function (n) {
                            u.files.push(new t(e.uid, n))
                        })
                    }, 999);
                    this.getRuntime()
                        .shimExec.call(this, "FileInput", "init", {
                            accept: n.accept
                            , multiple: n.multiple
                        });
                    this.trigger("ready")
                }
            };
            return n.FileInput = u
        });
        i("moxie/runtime/flash/file/FileReader", ["moxie/runtime/flash/Runtime", "moxie/core/utils/Encode"], function (n, t) {
            function i(n, i) {
                switch (i) {
                    case "readAsText":
                        return t.atob(n, "utf8");
                    case "readAsBinaryString":
                        return t.atob(n);
                    case "readAsDataURL":
                        return n
                }
                return null
            }
            var r = {
                read: function (n, t) {
                    var r = this;
                    return r.result = "", n === "readAsDataURL" && (r.result = "data:" + (t.type || "") + ";base64,"), r.bind("Progress", function (t, u) {
                        u && (r.result += i(u, n))
                    }, 999), r.getRuntime()
                        .shimExec.call(this, "FileReader", "readAsBase64", t.uid)
                }
            };
            return n.FileReader = r
        });
        i("moxie/runtime/flash/file/FileReaderSync", ["moxie/runtime/flash/Runtime", "moxie/core/utils/Encode"], function (n, t) {
            function i(n, i) {
                switch (i) {
                    case "readAsText":
                        return t.atob(n, "utf8");
                    case "readAsBinaryString":
                        return t.atob(n);
                    case "readAsDataURL":
                        return n
                }
                return null
            }
            var r = {
                read: function (n, t) {
                    var r, u = this.getRuntime();
                    return (r = u.shimExec.call(this, "FileReaderSync", "readAsBase64", t.uid), !r) ? null : (n === "readAsDataURL" && (r = "data:" + (t.type || "") + ";base64," + r), i(r, n, t.type))
                }
            };
            return n.FileReaderSync = r
        });
        i("moxie/runtime/flash/runtime/Transporter", ["moxie/runtime/flash/Runtime", "moxie/file/Blob"], function (n, t) {
            var i = {
                getAsBlob: function (n) {
                    var i = this.getRuntime()
                        , r = i.shimExec.call(this, "Transporter", "getAsBlob", n);
                    return r ? new t(i.uid, r) : null
                }
            };
            return n.Transporter = i
        });
        i("moxie/runtime/flash/xhr/XMLHttpRequest", ["moxie/runtime/flash/Runtime", "moxie/core/utils/Basic", "moxie/file/Blob", "moxie/file/File", "moxie/file/FileReaderSync", "moxie/runtime/flash/file/FileReaderSync", "moxie/xhr/FormData", "moxie/runtime/Transporter", "moxie/runtime/flash/runtime/Transporter"], function (n, t, i, r, u, f, e, o) {
            var s = {
                send: function (n, r) {
                    function s() {
                        n.transport = u.mode;
                        u.shimExec.call(f, "XMLHttpRequest", "send", n, r)
                    }

                    function l(n, t) {
                        u.shimExec.call(f, "XMLHttpRequest", "appendBlob", n, t.uid);
                        r = null;
                        s()
                    }

                    function a(n, t) {
                        var i = new o;
                        i.bind("TransportingComplete", function () {
                            t(this.result)
                        });
                        i.transport(n.getSource(), n.type, {
                            ruid: u.uid
                        })
                    }
                    var f = this
                        , u = f.getRuntime()
                        , c, h;
                    t.isEmptyObj(n.headers) || t.each(n.headers, function (n, t) {
                        u.shimExec.call(f, "XMLHttpRequest", "setRequestHeader", t, n.toString())
                    });
                    r instanceof e ? (r.each(function (n, t) {
                        n instanceof i ? c = t : u.shimExec.call(f, "XMLHttpRequest", "append", t, n)
                    }), r.hasBlob() ? (h = r.getBlob(), h.isDetached() ? a(h, function (n) {
                        h.destroy();
                        l(c, n)
                    }) : l(c, h)) : (r = null, s())) : r instanceof i ? r.isDetached() ? a(r, function (n) {
                        r.destroy();
                        r = n.uid;
                        s()
                    }) : (r = r.uid, s()) : s()
                }
                , getResponse: function (n) {
                    var f, i, e = this.getRuntime();
                    if (i = e.shimExec.call(this, "XMLHttpRequest", "getResponseAsBlob"), i) {
                        if (i = new r(e.uid, i), "blob" === n) return i;
                        try {
                            if (f = new u, !~t.inArray(n, ["", "text"])) {
                                if ("json" === n && !!window.JSON) return JSON.parse(f.readAsText(i))
                            } else return f.readAsText(i)
                        } finally {
                            i.destroy()
                        }
                    }
                    return null
                }
                , abort: function () {
                    var n = this.getRuntime();
                    n.shimExec.call(this, "XMLHttpRequest", "abort");
                    this.dispatchEvent("readystatechange");
                    this.dispatchEvent("abort")
                }
            };
            return n.XMLHttpRequest = s
        });
        i("moxie/runtime/flash/image/Image", ["moxie/runtime/flash/Runtime", "moxie/core/utils/Basic", "moxie/runtime/Transporter", "moxie/file/Blob", "moxie/file/FileReaderSync"], function (n, t, i, r, u) {
            var f = {
                loadFromBlob: function (n) {
                    function f(n) {
                        u.shimExec.call(r, "Image", "loadFromBlob", n.uid);
                        r = u = null
                    }
                    var r = this
                        , u = r.getRuntime()
                        , t;
                    n.isDetached() ? (t = new i, t.bind("TransportingComplete", function () {
                        f(t.result.getSource())
                    }), t.transport(n.getSource(), n.type, {
                        ruid: u.uid
                    })) : f(n.getSource())
                }
                , loadFromImage: function (n) {
                    var t = this.getRuntime();
                    return t.shimExec.call(this, "Image", "loadFromImage", n.uid)
                }
                , getInfo: function () {
                    var t = this.getRuntime()
                        , n = t.shimExec.call(this, "Image", "getInfo");
                    return n.meta && n.meta.thumb && n.meta.thumb.data && !(n.meta.thumb.data instanceof r) && (n.meta.thumb.data = new r(t.uid, n.meta.thumb.data)), n
                }
                , getAsBlob: function (n, t) {
                    var i = this.getRuntime()
                        , u = i.shimExec.call(this, "Image", "getAsBlob", n, t);
                    return u ? new r(i.uid, u) : null
                }
                , getAsDataURL: function () {
                    var r = this.getRuntime()
                        , n = r.Image.getAsBlob.apply(this, arguments)
                        , t, i;
                    return n ? (t = new u, i = t.readAsDataURL(n), n.destroy(), i) : null
                }
            };
            return n.Image = f
        });
        i("moxie/runtime/silverlight/Runtime", ["moxie/core/utils/Basic", "moxie/core/utils/Env", "moxie/core/utils/Dom", "moxie/core/Exceptions", "moxie/runtime/Runtime"], function (n, t, i, r, f) {
            function s(n) {
                var r = !1
                    , o = null
                    , u, t, i, f, s, e = 0
                    , h;
                try {
                    try {
                        o = new ActiveXObject("AgControl.AgControl");
                        o.IsVersionSupported(n) && (r = !0);
                        o = null
                    } catch (c) {
                        if (h = navigator.plugins["Silverlight Plug-In"], h) {
                            for (u = h.description, u === "1.0.30226.2" && (u = "2.0.30226.2"), t = u.split("."); t.length > 3;) t.pop();
                            while (t.length < 4) t.push(0);
                            for (i = n.split("."); i.length > 4;) i.pop();
                            do f = parseInt(i[e], 10), s = parseInt(t[e], 10), e++; while (e < i.length && f === s);
                            f <= s && !isNaN(f) && (r = !0)
                        }
                    }
                } catch (l) {
                    r = !1
                }
                return r
            }

            function h(h) {
                var c = this
                    , l;
                h = n.extend({
                    xap_url: t.xap_url
                }, h);
                f.call(this, h, e, {
                    access_binary: f.capTrue
                    , access_image_binary: f.capTrue
                    , display_media: f.capTest(u("moxie/image/Image"))
                    , do_cors: f.capTrue
                    , drag_and_drop: !1
                    , report_upload_progress: f.capTrue
                    , resize_image: f.capTrue
                    , return_response_headers: function (n) {
                        return n && c.mode === "client"
                    }
                    , return_response_type: function (n) {
                        return n !== "json" ? !0 : !!window.JSON
                    }
                    , return_status_code: function (t) {
                        return c.mode === "client" || !n.arrayDiff(t, [200, 404])
                    }
                    , select_file: f.capTrue
                    , select_multiple: f.capTrue
                    , send_binary_string: f.capTrue
                    , send_browser_cookies: function (n) {
                        return n && c.mode === "browser"
                    }
                    , send_custom_headers: function (n) {
                        return n && c.mode === "client"
                    }
                    , send_multipart: f.capTrue
                    , slice_blob: f.capTrue
                    , stream_upload: !0
                    , summon_file_dialog: !1
                    , upload_filesize: f.capTrue
                    , use_http_method: function (t) {
                        return c.mode === "client" || !n.arrayDiff(t, ["GET", "POST"])
                    }
                }, {
                    return_response_headers: function (n) {
                        return n ? "client" : "browser"
                    }
                    , return_status_code: function (t) {
                        return n.arrayDiff(t, [200, 404]) ? "client" : ["client", "browser"]
                    }
                    , send_browser_cookies: function (n) {
                        return n ? "browser" : "client"
                    }
                    , send_custom_headers: function (n) {
                        return n ? "client" : "browser"
                    }
                    , use_http_method: function (t) {
                        return n.arrayDiff(t, ["GET", "POST"]) ? "client" : ["client", "browser"]
                    }
                });
                s("2.0.31005.0") && t.browser !== "Opera" || (MXI_DEBUG && t.debug.runtime && t.log("\tSilverlight is not installed or minimal version (2.0.31005.0) requirement not met (not likely)."), this.mode = !1);
                n.extend(this, {
                    getShim: function () {
                        return i.get(this.uid)
                            .content.Moxie
                    }
                    , shimExec: function (n, t) {
                        var i = [].slice.call(arguments, 2);
                        return c.getShim()
                            .exec(this.uid, n, t, i)
                    }
                    , init: function () {
                        var n;
                        n = this.getShimContainer();
                        n.innerHTML = '<object id="' + this.uid + '" data="data:application/x-silverlight," type="application/x-silverlight-2" width="100%" height="100%" style="outline:none;"><param name="source" value="' + h.xap_url + '"/><param name="background" value="Transparent"/><param name="windowless" value="true"/><param name="enablehtmlaccess" value="true"/><param name="initParams" value="uid=' + this.uid + ",target=" + f.getGlobalEventTarget() + '"/><\/object>';
                        l = setTimeout(function () {
                            c && !c.initialized && (c.trigger("Error", new r.RuntimeError(r.RuntimeError.NOT_INIT_ERR)), MXI_DEBUG && t.debug.runtime && t.log("Silverlight failed to initialize within a specified period of time (5-10s)."))
                        }, t.OS !== "Windows" ? 1e4 : 5e3)
                    }
                    , destroy: function (n) {
                        return function () {
                            n.call(c);
                            clearTimeout(l);
                            h = l = n = c = null
                        }
                    }(this.destroy)
                }, o)
            }
            var e = "silverlight"
                , o = {};
            return f.addConstructor(e, h), o
        });
        i("moxie/runtime/silverlight/file/Blob", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Basic", "moxie/runtime/flash/file/Blob"], function (n, t, i) {
            return n.Blob = t.extend({}, i)
        });
        i("moxie/runtime/silverlight/file/FileInput", ["moxie/runtime/silverlight/Runtime", "moxie/file/File", "moxie/core/utils/Dom", "moxie/core/utils/Basic"], function (n, t, i, r) {
            function u(n) {
                for (var i = "", t = 0; t < n.length; t++) i += (i !== "" ? "|" : "") + n[t].title + " | *." + n[t].extensions.replace(/,/g, ";*.");
                return i
            }
            var f = {
                init: function (n) {
                    var f = this
                        , e = this.getRuntime()
                        , o = i.get(n.browse_button);
                    o && (o.setAttribute("tabindex", -1), o = null);
                    this.bind("Change", function () {
                        var n = e.shimExec.call(f, "FileInput", "getFiles");
                        f.files = [];
                        r.each(n, function (n) {
                            f.files.push(new t(e.uid, n))
                        })
                    }, 999);
                    e.shimExec.call(this, "FileInput", "init", u(n.accept), n.multiple);
                    this.trigger("ready")
                }
                , setOption: function (n, t) {
                    n == "accept" && (t = u(t));
                    this.getRuntime()
                        .shimExec.call(this, "FileInput", "setOption", n, t)
                }
            };
            return n.FileInput = f
        });
        i("moxie/runtime/silverlight/file/FileDrop", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Dom", "moxie/core/utils/Events"], function (n, t, i) {
            var r = {
                init: function () {
                    var n = this
                        , r = n.getRuntime()
                        , u;
                    return u = r.getShimContainer(), i.addEvent(u, "dragover", function (n) {
                        n.preventDefault();
                        n.stopPropagation();
                        n.dataTransfer.dropEffect = "copy"
                    }, n.uid), i.addEvent(u, "dragenter", function (n) {
                        n.preventDefault();
                        var i = t.get(r.uid)
                            .dragEnter(n);
                        i && n.stopPropagation()
                    }, n.uid), i.addEvent(u, "drop", function (n) {
                        n.preventDefault();
                        var i = t.get(r.uid)
                            .dragDrop(n);
                        i && n.stopPropagation()
                    }, n.uid), r.shimExec.call(this, "FileDrop", "init")
                }
            };
            return n.FileDrop = r
        });
        i("moxie/runtime/silverlight/file/FileReader", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Basic", "moxie/runtime/flash/file/FileReader"], function (n, t, i) {
            return n.FileReader = t.extend({}, i)
        });
        i("moxie/runtime/silverlight/file/FileReaderSync", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Basic", "moxie/runtime/flash/file/FileReaderSync"], function (n, t, i) {
            return n.FileReaderSync = t.extend({}, i)
        });
        i("moxie/runtime/silverlight/runtime/Transporter", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Basic", "moxie/runtime/flash/runtime/Transporter"], function (n, t, i) {
            return n.Transporter = t.extend({}, i)
        });
        i("moxie/runtime/silverlight/xhr/XMLHttpRequest", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Basic", "moxie/runtime/flash/xhr/XMLHttpRequest", "moxie/runtime/silverlight/file/FileReaderSync", "moxie/runtime/silverlight/runtime/Transporter"], function (n, t, i) {
            return n.XMLHttpRequest = t.extend({}, i)
        });
        i("moxie/runtime/silverlight/image/Image", ["moxie/runtime/silverlight/Runtime", "moxie/core/utils/Basic", "moxie/file/Blob", "moxie/runtime/flash/image/Image"], function (n, t, i, r) {
            return n.Image = t.extend({}, r, {
                getInfo: function () {
                    var u = this.getRuntime()
                        , n = {
                            meta: {}
                        }
                        , r = u.shimExec.call(this, "Image", "getInfo");
                    return r.meta && (t.each(["tiff", "exif", "gps", "thumb"], function (t) {
                        var u = r.meta[t]
                            , e, f, o, i;
                        if (u && u.keys)
                            for (n.meta[t] = {}, f = 0, o = u.keys.length; f < o; f++) e = u.keys[f], i = u[e], i && (/^(\d|[1-9]\d+)$/.test(i) ? i = parseInt(i, 10) : /^\d*\.\d+$/.test(i) && (i = parseFloat(i)), n.meta[t][e] = i)
                    }), n.meta && n.meta.thumb && n.meta.thumb.data && !(n.meta.thumb.data instanceof i) && (n.meta.thumb.data = new i(u.uid, n.meta.thumb.data))), n.width = parseInt(r.width, 10), n.height = parseInt(r.height, 10), n.size = parseInt(r.size, 10), n.type = r.type, n.name = r.name, n
                }
                , resize: function (n, t, i) {
                    this.getRuntime()
                        .shimExec.call(this, "Image", "resize", n.x, n.y, n.width, n.height, t, i.preserveHeaders, i.resample)
                }
            })
        });
        i("moxie/runtime/html4/Runtime", ["moxie/core/utils/Basic", "moxie/core/Exceptions", "moxie/runtime/Runtime", "moxie/core/utils/Env"], function (n, t, i, r) {
            function o(t) {
                var o = this
                    , s = i.capTest
                    , h = i.capTrue;
                i.call(this, t, e, {
                    access_binary: s(window.FileReader || window.File && File.getAsDataURL)
                    , access_image_binary: !1
                    , display_media: s((r.can("create_canvas") || r.can("use_data_uri_over32kb")) && u("moxie/image/Image"))
                    , do_cors: !1
                    , drag_and_drop: !1
                    , filter_by_extension: s(function () {
                        return !(r.browser === "Chrome" && r.verComp(r.version, 28, "<") || r.browser === "IE" && r.verComp(r.version, 10, "<") || r.browser === "Safari" && r.verComp(r.version, 7, "<") || r.browser === "Firefox" && r.verComp(r.version, 37, "<"))
                    }())
                    , resize_image: function () {
                        return f.Image && o.can("access_binary") && r.can("create_canvas")
                    }
                    , report_upload_progress: !1
                    , return_response_headers: !1
                    , return_response_type: function (t) {
                        return t === "json" && !!window.JSON ? !0 : !!~n.inArray(t, ["text", "document", ""])
                    }
                    , return_status_code: function (t) {
                        return !n.arrayDiff(t, [200, 404])
                    }
                    , select_file: function () {
                        return r.can("use_fileinput")
                    }
                    , select_multiple: !1
                    , send_binary_string: !1
                    , send_custom_headers: !1
                    , send_multipart: !0
                    , slice_blob: !1
                    , stream_upload: function () {
                        return o.can("select_file")
                    }
                    , summon_file_dialog: function () {
                        return o.can("select_file") && !(r.browser === "Firefox" && r.verComp(r.version, 4, "<") || r.browser === "Opera" && r.verComp(r.version, 12, "<") || r.browser === "IE" && r.verComp(r.version, 10, "<") || r.os === "iOS" || r.os === "Android")
                    }
                    , upload_filesize: h
                    , use_http_method: function (t) {
                        return !n.arrayDiff(t, ["GET", "POST"])
                    }
                });
                n.extend(this, {
                    init: function () {
                        this.trigger("Init")
                    }
                    , destroy: function (n) {
                        return function () {
                            n.call(o);
                            n = o = null
                        }
                    }(this.destroy)
                });
                n.extend(this.getShim(), f)
            }
            var e = "html4"
                , f = {};
            return i.addConstructor(e, o), f
        });
        i("moxie/runtime/html4/file/FileInput", ["moxie/runtime/html4/Runtime", "moxie/file/File", "moxie/core/utils/Basic", "moxie/core/utils/Dom", "moxie/core/utils/Events", "moxie/core/utils/Mime", "moxie/core/utils/Env"], function (n, t, i, r, u, f, e) {
            function o() {
                function c() {
                    var l = this
                        , v = l.getRuntime()
                        , w, y, a, h, f, p;
                    p = i.guid("uid_");
                    w = v.getShimContainer();
                    o && (a = r.get(o + "_form"), a && (i.extend(a.style, {
                        top: "100%"
                    }), a.firstChild.setAttribute("tabindex", -1)));
                    h = document.createElement("form");
                    h.setAttribute("id", p + "_form");
                    h.setAttribute("method", "post");
                    h.setAttribute("enctype", "multipart/form-data");
                    h.setAttribute("encoding", "multipart/form-data");
                    i.extend(h.style, {
                        overflow: "hidden"
                        , position: "absolute"
                        , top: 0
                        , left: 0
                        , width: "100%"
                        , height: "100%"
                    });
                    f = document.createElement("input");
                    f.setAttribute("id", p);
                    f.setAttribute("type", "file");
                    f.setAttribute("accept", s.join(","));
                    v.can("summon_file_dialog") && f.setAttribute("tabindex", -1);
                    i.extend(f.style, {
                        fontSize: "999px"
                        , opacity: 0
                    });
                    h.appendChild(f);
                    w.appendChild(h);
                    i.extend(f.style, {
                        position: "absolute"
                        , top: 0
                        , left: 0
                        , width: "100%"
                        , height: "100%"
                    });
                    e.browser === "IE" && e.verComp(e.version, 10, "<") && i.extend(f.style, {
                        filter: "progid:DXImageTransform.Microsoft.Alpha(opacity=0)"
                    });
                    f.onchange = function () {
                        var n;
                        this.value && (n = this.files ? this.files[0] : {
                            name: this.value
                        }, n = new t(v.uid, n), this.onchange = function () { }, c.call(l), l.files = [n], f.setAttribute("id", n.uid), h.setAttribute("id", n.uid + "_form"), l.trigger("change"), f = h = null)
                    };
                    v.can("summon_file_dialog") && (y = r.get(n.browse_button), u.removeEvent(y, "click", l.uid), u.addEvent(y, "click", function (n) {
                        f && !f.disabled && f.click();
                        n.preventDefault()
                    }, l.uid));
                    o = p;
                    w = a = y = null
                }
                var o, s = []
                    , n, h;
                i.extend(this, {
                    init: function (t) {
                        var i = this
                            , e = i.getRuntime()
                            , o;
                        n = t;
                        s = f.extList2mimes(t.accept, e.can("filter_by_extension"));
                        o = e.getShimContainer()
                            , function () {
                                var f, c, s;
                                f = r.get(t.browse_button);
                                h = r.getStyle(f, "z-index") || "auto";
                                e.can("summon_file_dialog") ? (r.getStyle(f, "position") === "static" && (f.style.position = "relative"), i.bind("Refresh", function () {
                                    c = parseInt(h, 10) || 1;
                                    r.get(n.browse_button)
                                        .style.zIndex = c;
                                    this.getRuntime()
                                        .getShimContainer()
                                        .style.zIndex = c - 1
                                })) : f.setAttribute("tabindex", -1);
                                s = e.can("summon_file_dialog") ? f : o;
                                u.addEvent(s, "mouseover", function () {
                                    i.trigger("mouseenter")
                                }, i.uid);
                                u.addEvent(s, "mouseout", function () {
                                    i.trigger("mouseleave")
                                }, i.uid);
                                u.addEvent(s, "mousedown", function () {
                                    i.trigger("mousedown")
                                }, i.uid);
                                u.addEvent(r.get(t.container), "mouseup", function () {
                                    i.trigger("mouseup")
                                }, i.uid);
                                f = null
                            }();
                        c.call(this);
                        o = null;
                        i.trigger({
                            type: "ready"
                            , async: !0
                        })
                    }
                    , setOption: function (n, t) {
                        var u = this.getRuntime()
                            , i;
                        n == "accept" && (s = t.mimes || f.extList2mimes(t, u.can("filter_by_extension")));
                        i = r.get(o);
                        i && i.setAttribute("accept", s.join(","))
                    }
                    , disable: function (n) {
                        var t;
                        (t = r.get(o)) && (t.disabled = !!n)
                    }
                    , destroy: function () {
                        var e = this.getRuntime()
                            , c = e.getShim()
                            , t = e.getShimContainer()
                            , f = n && r.get(n.container)
                            , i = n && r.get(n.browse_button);
                        f && u.removeAllEvents(f, this.uid);
                        i && (u.removeAllEvents(i, this.uid), i.style.zIndex = h);
                        t && (u.removeAllEvents(t, this.uid), t.innerHTML = "");
                        c.removeInstance(this.uid);
                        o = s = n = t = f = i = c = null
                    }
                })
            }
            return n.FileInput = o
        });
        i("moxie/runtime/html4/file/FileReader", ["moxie/runtime/html4/Runtime", "moxie/runtime/html5/file/FileReader"], function (n, t) {
            return n.FileReader = t
        });
        i("moxie/runtime/html4/xhr/XMLHttpRequest", ["moxie/runtime/html4/Runtime", "moxie/core/utils/Basic", "moxie/core/utils/Dom", "moxie/core/utils/Url", "moxie/core/Exceptions", "moxie/core/utils/Events", "moxie/file/Blob", "moxie/xhr/FormData"], function (n, t, i, r, u, f, e, o) {
            function s() {
                function c(t) {
                    var o = this
                        , s, r, u, e, h = !1;
                    if (n) {
                        if (s = n.id.replace(/_iframe$/, ""), r = i.get(s + "_form"), r) {
                            for (u = r.getElementsByTagName("input"), e = u.length; e--;) switch (u[e].getAttribute("type")) {
                                case "hidden":
                                    u[e].parentNode.removeChild(u[e]);
                                    break;
                                case "file":
                                    h = !0
                            }
                            u = [];
                            h || r.parentNode.removeChild(r);
                            r = null
                        }
                        setTimeout(function () {
                            f.removeEvent(n, "load", o.uid);
                            n.parentNode && n.parentNode.removeChild(n);
                            var i = o.getRuntime()
                                .getShimContainer();
                            i.children.length || i.parentNode.removeChild(i);
                            i = n = null;
                            t()
                        }, 1)
                    }
                }
                var h, s, n;
                t.extend(this, {
                    send: function (l, a) {
                        function d() {
                            var u = k.getShimContainer() || document.body
                                , i = document.createElement("div");
                            i.innerHTML = '<iframe id="' + p + '_iframe" name="' + p + '_iframe" src="javascript:&quot;&quot;" style="display:none"><\/iframe>';
                            n = i.firstChild;
                            u.appendChild(n);
                            f.addEvent(n, "load", function () {
                                var i, u;
                                try {
                                    i = n.contentWindow.document || n.contentDocument || window.frames[n.id].document;
                                    u = i.title.match(/(4(0[0-9]|1[0-7]|2[2346]))\b/);
                                    u ? (h = +u[0], s = i.documentElement.innerHTML) : (h = 200, s = t.trim(i.body.innerHTML), y.trigger({
                                        type: "progress"
                                        , loaded: s.length
                                        , total: s.length
                                    }), b && y.trigger({
                                        type: "uploadprogress"
                                        , loaded: b.size || 1025
                                        , total: b.size || 1025
                                    }))
                                } catch (f) {
                                    if (r.hasSameOrigin(l.url)) h = 404;
                                    else {
                                        c.call(y, function () {
                                            y.trigger("error")
                                        });
                                        return
                                    }
                                }
                                c.call(y, function () {
                                    y.trigger("load")
                                })
                            }, y.uid)
                        }
                        var y = this
                            , k = y.getRuntime()
                            , p, v, w, b;
                        if (h = s = null, a instanceof o && a.hasBlob()) {
                            if (b = a.getBlob(), p = b.uid, w = i.get(p), v = i.get(p + "_form"), !v) throw new u.DOMException(u.DOMException.NOT_FOUND_ERR);
                        } else p = t.guid("uid_"), v = document.createElement("form"), v.setAttribute("id", p + "_form"), v.setAttribute("method", l.method), v.setAttribute("enctype", "multipart/form-data"), v.setAttribute("encoding", "multipart/form-data"), k.getShimContainer()
                            .appendChild(v);
                        v.setAttribute("target", p + "_iframe");
                        a instanceof o && a.each(function (n, i) {
                            if (n instanceof e) w && w.setAttribute("name", i);
                            else {
                                var r = document.createElement("input");
                                t.extend(r, {
                                    type: "hidden"
                                    , name: i
                                    , value: n
                                });
                                w ? v.insertBefore(r, w) : v.appendChild(r)
                            }
                        });
                        v.setAttribute("action", l.url);
                        d();
                        v.submit();
                        y.trigger("loadstart")
                    }
                    , getStatus: function () {
                        return h
                    }
                    , getResponse: function (n) {
                        if ("json" === n) {
                            if (t.typeOf(s) === "string" && !!window.JSON) try {
                                return JSON.parse(s.replace(/^\s*<pre[^>]*>/, "")
                                    .replace(/<\/pre>\s*$/, ""))
                            } catch (i) {
                                return null
                            }
                        } else "document" === n;
                        return s
                    }
                    , abort: function () {
                        var t = this;
                        n && n.contentWindow && (n.contentWindow.stop ? n.contentWindow.stop() : n.contentWindow.document.execCommand ? n.contentWindow.document.execCommand("Stop") : n.src = "about:blank");
                        c.call(this, function () {
                            t.dispatchEvent("abort")
                        })
                    }
                    , destroy: function () {
                        var n = this.getRuntime();
                        n && n.getShim()
                            .removeInstance(this.uid)
                    }
                })
            }
            return n.XMLHttpRequest = s
        });
        i("moxie/runtime/html4/image/Image", ["moxie/runtime/html4/Runtime", "moxie/runtime/html5/image/Image"], function (n, t) {
            return n.Image = t
        });
        o(["moxie/core/utils/Basic", "moxie/core/utils/Encode", "moxie/core/utils/Env", "moxie/core/Exceptions", "moxie/core/utils/Dom", "moxie/core/EventTarget", "moxie/runtime/Runtime", "moxie/runtime/RuntimeClient", "moxie/file/Blob", "moxie/core/I18n", "moxie/core/utils/Mime", "moxie/file/FileInput", "moxie/file/File", "moxie/file/FileDrop", "moxie/file/FileReader", "moxie/core/utils/Url", "moxie/runtime/RuntimeTarget", "moxie/xhr/FormData", "moxie/xhr/XMLHttpRequest", "moxie/image/Image", "moxie/core/utils/Events", "moxie/runtime/html5/image/ResizerCanvas"])
    })(this)
});
(function (n, t) {
    var i = function () {
        var n = {};
        return t.apply(n, arguments), n.plupload
    };
    typeof define == "function" && define.amd ? define("plupload", ["./moxie"], i) : typeof module == "object" && module.exports ? module.exports = i(require("./moxie")) : n.plupload = i(n.moxie)
})(this || window, function (n) {
    (function (n, t, i) {
        function s(n) {
            function u(n, i, r) {
                var u = {
                    chunks: "slice_blob"
                    , jpgresize: "send_binary_string"
                    , pngresize: "send_binary_string"
                    , progress: "report_upload_progress"
                    , multi_selection: "select_multiple"
                    , dragdrop: "drag_and_drop"
                    , drop_element: "drag_and_drop"
                    , headers: "send_custom_headers"
                    , urlstream_upload: "send_binary_string"
                    , canSendBinary: "send_binary"
                    , triggerDialog: "summon_file_dialog"
                };
                u[n] ? t[u[n]] = i : r || (t[n] = i)
            }
            var i = n.required_features
                , t = {};
            return typeof i == "string" ? r.each(i.split(/\s*,\s*/), function (n) {
                u(n, !0)
            }) : typeof i == "object" ? r.each(i, function (n, t) {
                u(t, n)
            }) : i === !0 && (n.chunk_size && n.chunk_size > 0 && (t.slice_blob = !0), r.isEmptyObj(n.resize) && n.multipart !== !1 || (t.send_binary_string = !0), n.http_method && (t.use_http_method = n.http_method), r.each(n, function (n, t) {
                u(t, !!n, !0)
            })), t
        }
        var e = window.setTimeout
            , o = {}
            , u = t.core.utils
            , f = t.runtime.Runtime
            , r = {
                VERSION: "2.3.6"
                , STOPPED: 1
                , STARTED: 2
                , QUEUED: 1
                , UPLOADING: 2
                , FAILED: 4
                , DONE: 5
                , GENERIC_ERROR: -100
                , HTTP_ERROR: -200
                , IO_ERROR: -300
                , SECURITY_ERROR: -400
                , INIT_ERROR: -500
                , FILE_SIZE_ERROR: -600
                , FILE_EXTENSION_ERROR: -601
                , FILE_DUPLICATE_ERROR: -602
                , IMAGE_FORMAT_ERROR: -700
                , MEMORY_ERROR: -701
                , IMAGE_DIMENSIONS_ERROR: -702
                , moxie: t
                , mimeTypes: u.Mime.mimes
                , ua: u.Env
                , typeOf: u.Basic.typeOf
                , extend: u.Basic.extend
                , guid: u.Basic.guid
                , getAll: function (n) {
                    var t = []
                        , i, u;
                    for (r.typeOf(n) !== "array" && (n = [n]), u = n.length; u--;) i = r.get(n[u]), i && t.push(i);
                    return t.length ? t : null
                }
                , get: u.Dom.get
                , each: u.Basic.each
                , getPos: u.Dom.getPos
                , getSize: u.Dom.getSize
                , xmlEncode: function (n) {
                    var t = {
                        "<": "lt"
                        , ">": "gt"
                        , "&": "amp"
                        , '"': "quot"
                        , "'": "#39"
                    };
                    return n ? ("" + n)
                        .replace(/[<>&\"\']/g, function (n) {
                            return t[n] ? "&" + t[n] + ";" : n
                        }) : n
                }
                , toArray: u.Basic.toArray
                , inArray: u.Basic.inArray
                , inSeries: u.Basic.inSeries
                , addI18n: t.core.I18n.addI18n
                , translate: t.core.I18n.translate
                , sprintf: u.Basic.sprintf
                , isEmptyObj: u.Basic.isEmptyObj
                , hasClass: u.Dom.hasClass
                , addClass: u.Dom.addClass
                , removeClass: u.Dom.removeClass
                , getStyle: u.Dom.getStyle
                , addEvent: u.Events.addEvent
                , removeEvent: u.Events.removeEvent
                , removeAllEvents: u.Events.removeAllEvents
                , cleanName: function (n) {
                    for (var i = [/[\300-\306]/g, "A", /[\340-\346]/g, "a", /\307/g, "C", /\347/g, "c", /[\310-\313]/g, "E", /[\350-\353]/g, "e", /[\314-\317]/g, "I", /[\354-\357]/g, "i", /\321/g, "N", /\361/g, "n", /[\322-\330]/g, "O", /[\362-\370]/g, "o", /[\331-\334]/g, "U", /[\371-\374]/g, "u"], t = 0; t < i.length; t += 2) n = n.replace(i[t], i[t + 1]);
                    return n = n.replace(/\s+/g, "_"), n.replace(/[^a-z0-9_\-\.]+/gi, "")
                }
                , buildUrl: function (n, t) {
                    var i = "";
                    return r.each(t, function (n, t) {
                        i += (i ? "&" : "") + encodeURIComponent(t) + "=" + encodeURIComponent(n)
                    }), i && (n += (n.indexOf("?") > 0 ? "&" : "?") + i), n
                }
                , formatSize: function (n) {
                    function u(n, t) {
                        return Math.round(n * Math.pow(10, t)) / Math.pow(10, t)
                    }
                    if (n === i || /\D/.test(n)) return r.translate("N/A");
                    var t = Math.pow(1024, 4);
                    return n > t ? u(n / t, 1) + " " + r.translate("tb") : n > (t /= 1024) ? u(n / t, 1) + " " + r.translate("gb") : n > (t /= 1024) ? u(n / t, 1) + " " + r.translate("mb") : n > 1024 ? Math.round(n / 1024) + " " + r.translate("kb") : n + " " + r.translate("b")
                }
                , parseSize: u.Basic.parseSizeStr
                , predictRuntime: function (n, t) {
                    var i, u;
                    return i = new r.Uploader(n), u = f.thatCan(i.getOption()
                        .required_features, t || n.runtimes), i.destroy(), u
                }
                , addFileFilter: function (n, t) {
                    o[n] = t
                }
            };
        r.addFileFilter("mime_types", function (n, t, i) {
            n.length && n.regexp && !n.regexp.test(t.name) ? (this.trigger("Error", {
                code: r.FILE_EXTENSION_ERROR
                , message: r.translate("File extension error.")
                , file: t
            }), i(!1)) : i(!0)
        });
        r.addFileFilter("max_file_size", function (n, t, i) {
            var u;
            n = r.parseSize(n);
            t.size !== u && n && t.size > n ? (this.trigger("Error", {
                code: r.FILE_SIZE_ERROR
                , message: r.translate("File size error.")
                , file: t
            }), i(!1)) : i(!0)
        });
        r.addFileFilter("prevent_duplicates", function (n, t, i) {
            if (n)
                for (var u = this.files.length; u--;)
                    if (t.name === this.files[u].name && t.size === this.files[u].size) {
                        this.trigger("Error", {
                            code: r.FILE_DUPLICATE_ERROR
                            , message: r.translate("Duplicate file error.")
                            , file: t
                        });
                        i(!1);
                        return
                    } i(!0)
        });
        r.addFileFilter("prevent_empty", function (n, t, u) {
            n && !t.size && t.size !== i ? (this.trigger("Error", {
                code: r.FILE_SIZE_ERROR
                , message: r.translate("File size error.")
                , file: t
            }), u(!1)) : u(!0)
        });
        r.Uploader = function (n) {
            function k() {
                var n, i = 0
                    , t;
                if (this.state == r.STARTED) {
                    for (t = 0; t < c.length; t++) n || c[t].status != r.QUEUED ? i++ : (n = c[t], this.trigger("BeforeUpload", n) && (n.status = r.UPLOADING, this.trigger("UploadFile", n)));
                    i == c.length && (this.state !== r.STOPPED && (this.state = r.STOPPED, this.trigger("StateChanged")), this.trigger("UploadComplete", c))
                }
            }

            function nt(n) {
                n.percent = n.size > 0 ? Math.ceil(n.loaded / n.size * 100) : 100;
                b()
            }

            function b() {
                var t, n, u, f = 0;
                for (l.reset(), t = 0; t < c.length; t++) n = c[t], n.size !== i ? (l.size += n.origSize, u = n.loaded * n.origSize / n.size, (!n.completeTimestamp || n.completeTimestamp > w) && (f += u), l.loaded += u) : l.size = i, n.status == r.DONE ? l.uploaded++ : n.status == r.FAILED ? l.failed++ : l.queued++;
                l.size === i ? l.percent = c.length > 0 ? Math.ceil(l.uploaded / c.length * 100) : 0 : (l.bytesPerSec = Math.ceil(f / ((+new Date - w || 1) / 1e3)), l.percent = l.size > 0 ? Math.ceil(l.loaded / l.size * 100) : 0)
            }

            function d() {
                var n = a[0] || v[0];
                return n ? n.getRuntime()
                    .uid : !1
            }

            function rt() {
                this.bind("FilesAdded FilesRemoved", function (n) {
                    n.trigger("QueueChanged");
                    n.refresh()
                });
                this.bind("CancelUpload", ht);
                this.bind("BeforeUpload", ft);
                this.bind("UploadFile", et);
                this.bind("UploadProgress", ot);
                this.bind("StateChanged", st);
                this.bind("QueueChanged", b);
                this.bind("Error", lt);
                this.bind("FileUploaded", ct);
                this.bind("Destroy", at)
            }

            function tt(n, i) {
                var u = this
                    , e = 0
                    , o = []
                    , s = {
                        runtime_order: n.runtimes
                        , required_caps: n.required_features
                        , preferred_caps: p
                        , swf_url: n.flash_swf_url
                        , xap_url: n.silverlight_xap_url
                    };
                r.each(n.runtimes.split(/\s*,\s*/), function (t) {
                    n[t] && (s[t] = n[t])
                });
                n.browse_button && r.each(n.browse_button, function (i) {
                    o.push(function (o) {
                        var h = new t.file.FileInput(r.extend({}, s, {
                            accept: n.filters.mime_types
                            , name: n.file_data_name
                            , multiple: n.multi_selection
                            , container: n.container
                            , browse_button: i
                        }));
                        h.onready = function () {
                            var n = f.getInfo(this.ruid);
                            r.extend(u.features, {
                                chunks: n.can("slice_blob")
                                , multipart: n.can("send_multipart")
                                , multi_selection: n.can("select_multiple")
                            });
                            e++;
                            a.push(this);
                            o()
                        };
                        h.onchange = function () {
                            u.addFile(this.files)
                        };
                        h.bind("mouseenter mouseleave mousedown mouseup", function (t) {
                            y || (n.browse_button_hover && ("mouseenter" === t.type ? r.addClass(i, n.browse_button_hover) : "mouseleave" === t.type && r.removeClass(i, n.browse_button_hover)), n.browse_button_active && ("mousedown" === t.type ? r.addClass(i, n.browse_button_active) : "mouseup" === t.type && r.removeClass(i, n.browse_button_active)))
                        });
                        h.bind("mousedown", function () {
                            u.trigger("Browse")
                        });
                        h.bind("error runtimeerror", function () {
                            h = null;
                            o()
                        });
                        h.init()
                    })
                });
                n.drop_element && r.each(n.drop_element, function (n) {
                    o.push(function (i) {
                        var o = new t.file.FileDrop(r.extend({}, s, {
                            drop_zone: n
                        }));
                        o.onready = function () {
                            var n = f.getInfo(this.ruid);
                            r.extend(u.features, {
                                chunks: n.can("slice_blob")
                                , multipart: n.can("send_multipart")
                                , dragdrop: n.can("drag_and_drop")
                            });
                            e++;
                            v.push(this);
                            i()
                        };
                        o.ondrop = function () {
                            u.addFile(this.files)
                        };
                        o.bind("error runtimeerror", function () {
                            o = null;
                            i()
                        });
                        o.init()
                    })
                });
                r.inSeries(o, function () {
                    typeof i == "function" && i(e)
                })
            }

            function ut(n, r, u, f) {
                var e = new t.image.Image;
                try {
                    e.onload = function () {
                        r.width > this.width && r.height > this.height && r.quality === i && r.preserve_headers && !r.crop ? (this.destroy(), f(n)) : e.downsize(r.width, r.height, r.crop, r.preserve_headers)
                    };
                    e.onresize = function () {
                        var t = this.getAsBlob(n.type, r.quality);
                        this.destroy();
                        f(t)
                    };
                    e.bind("error runtimeerror", function () {
                        this.destroy();
                        f(n)
                    });
                    e.load(n, u)
                } catch (o) {
                    f(n)
                }
            }

            function it(n, i, e) {
                function c(n, i, f) {
                    var e = u[n];
                    switch (n) {
                        case "max_file_size":
                            n === "max_file_size" && (u.max_file_size = u.filters.max_file_size = i);
                            break;
                        case "chunk_size":
                            (i = r.parseSize(i)) && (u[n] = i, u.send_file_name = !0);
                            break;
                        case "multipart":
                            u[n] = i;
                            i || (u.send_file_name = !0);
                            break;
                        case "http_method":
                            u[n] = i.toUpperCase() === "PUT" ? "PUT" : "POST";
                            break;
                        case "unique_names":
                            u[n] = i;
                            i && (u.send_file_name = !0);
                            break;
                        case "filters":
                            r.typeOf(i) === "array" && (i = {
                                mime_types: i
                            });
                            f ? r.extend(u.filters, i) : u.filters = i;
                            i.mime_types && (r.typeOf(i.mime_types) === "string" && (i.mime_types = t.core.utils.Mime.mimes2extList(i.mime_types)), i.skipMimeTypesCheck || (i.mime_types.regexp = function (n) {
                                var t = [];
                                return r.each(n, function (n) {
                                    r.each(n.extensions.split(/,/), function (n) {
                                        /^\s*\*\s*$/.test(n) ? t.push("\\.*") : t.push("\\." + n.replace(new RegExp("[" + "/^$.*+?|()[]{}\\".replace(/./g, "\\$&") + "]", "g"), "\\$&"))
                                    })
                                }), new RegExp("(" + t.join("|") + ")$", "i")
                            }(i.mime_types)), u.filters.mime_types = i.mime_types, f || a.length && r.each(a, function (n) {
                                n.setOption("accept", u.filters.mime_types)
                            }));
                            break;
                        case "resize":
                            u.resize = i ? r.extend({
                                preserve_headers: !0
                                , crop: !1
                            }, i) : !1;
                            break;
                        case "prevent_duplicates":
                            u.prevent_duplicates = u.filters.prevent_duplicates = !!i;
                            break;
                        case "container":
                        case "browse_button":
                        case "drop_element":
                            i = "container" === n ? r.get(i) : r.getAll(i);
                        case "runtimes":
                        case "multi_selection":
                        case "flash_swf_url":
                        case "silverlight_xap_url":
                            u[n] = i;
                            f || (h = !0);
                            break;
                        default:
                            u[n] = i
                    }
                    f || o.trigger("OptionChanged", n, i, e)
                }
                var o = this
                    , h = !1;
                typeof n == "object" ? r.each(n, function (n, t) {
                    c(t, n, e)
                }) : c(n, i, e);
                e ? (u.required_features = s(r.extend({}, u)), p = s(r.extend({}, u, {
                    required_features: !0
                }))) : h && (o.trigger("Destroy"), tt.call(o, u, function (n) {
                    n ? (o.runtime = f.getInfo(d())
                        .type, o.trigger("Init", {
                            runtime: o.runtime
                        }), o.trigger("PostInit")) : o.trigger("Error", {
                            code: r.INIT_ERROR
                            , message: r.translate("Init error.")
                        })
                }))
            }

            function ft(n, t) {
                if (n.settings.unique_names) {
                    var i = t.name.match(/\.([^.]+)$/)
                        , r = "part";
                    i && (r = i[1]);
                    t.target_name = t.id + "." + r
                }
            }

            function et(n, i) {
                function y() {
                    v-- > 0 ? e(c, 1e3) : (i.loaded = f, n.trigger("Error", {
                        code: r.HTTP_ERROR
                        , message: r.translate("HTTP Error.")
                        , file: i
                        , response: h.responseText
                        , status: h.status
                        , responseHeaders: h.getAllResponseHeaders()
                        , xhr: h
                    }))
                }

                function c() {
                    var e, t = {}
                        , s;
                    i.status === r.UPLOADING && n.state !== r.STOPPED && (n.settings.send_file_name && (t.name = i.target_name || i.name), o && l.chunks && u.size > o ? (s = Math.min(o, u.size - f), e = u.slice(f, f + s)) : (s = u.size, e = u), o && l.chunks && (n.settings.send_chunk_number ? (t.chunk = Math.ceil(f / o), t.chunks = Math.ceil(u.size / o)) : (t.offset = f, t.total = u.size)), n.trigger("BeforeChunkUpload", i, t, e, f) && w(t, e, s))
                }

                function w(o, p, w) {
                    var b;
                    h = new t.xhr.XMLHttpRequest;
                    h.upload && (h.upload.onprogress = function (t) {
                        i.loaded = Math.min(i.size, f + t.loaded);
                        n.trigger("UploadProgress", i)
                    });
                    h.onload = function () {
                        if (h.status < 200 || h.status >= 400) {
                            y();
                            return
                        }
                        v = n.settings.max_retries;
                        w < u.size ? (p.destroy(), f += w, i.loaded = Math.min(f, u.size), n.trigger("ChunkUploaded", i, {
                            offset: i.loaded
                            , total: u.size
                            , response: h.responseText
                            , status: h.status
                            , responseHeaders: h.getAllResponseHeaders()
                            , xhr: h
                        }), r.ua.browser === "Android Browser" && n.trigger("UploadProgress", i)) : i.loaded = i.size;
                        p = b = null;
                        !f || f >= u.size ? (i.size != i.origSize && (u.destroy(), u = null), n.trigger("UploadProgress", i), i.status = r.DONE, i.completeTimestamp = +new Date, n.trigger("FileUploaded", i, {
                            response: h.responseText
                            , status: h.status
                            , responseHeaders: h.getAllResponseHeaders()
                            , xhr: h
                        })) : e(c, 1)
                    };
                    h.onerror = function () {
                        y()
                    };
                    h.onloadend = function () {
                        this.destroy()
                    };
                    n.settings.multipart && l.multipart ? (s = r.buildUrl(n.settings.url, r.extend(o, n.settings.multipart_params)), h.open(n.settings.http_method, s, !0), r.each(n.settings.headers, function (n, t) {
                        h.setRequestHeader(t, n)
                    }), b = new t.xhr.FormData, b.append(n.settings.file_data_name, p), h.send(b, a)) : (s = r.buildUrl(n.settings.url, r.extend(o, n.settings.multipart_params)), h.open(n.settings.http_method, s, !0), r.each(n.settings.headers, function (n, t) {
                        h.setRequestHeader(t, n)
                    }), h.hasRequestHeader("Content-Type") || h.setRequestHeader("Content-Type", "application/octet-stream"), h.send(p, a))
                }
                var s = n.settings.url
                    , o = n.settings.chunk_size
                    , v = n.settings.max_retries
                    , l = n.features
                    , f = 0
                    , u, a = {
                        runtime_order: n.settings.runtimes
                        , required_caps: n.settings.required_features
                        , preferred_caps: p
                        , swf_url: n.settings.flash_swf_url
                        , xap_url: n.settings.silverlight_xap_url
                    };
                i.loaded && (f = i.loaded = o ? o * Math.floor(i.loaded / o) : 0);
                u = i.getSource();
                r.isEmptyObj(n.settings.resize) || r.inArray(u.type, ["image/jpeg", "image/png"]) === -1 ? c() : ut(u, n.settings.resize, a, function (n) {
                    u = n;
                    i.size = n.size;
                    c()
                })
            }

            function ot(n, t) {
                nt(t)
            }

            function st(n) {
                if (n.state == r.STARTED) w = +new Date;
                else if (n.state == r.STOPPED)
                    for (var t = n.files.length - 1; t >= 0; t--) n.files[t].status == r.UPLOADING && (n.files[t].status = r.QUEUED, b())
            }

            function ht() {
                h && h.abort()
            }

            function ct(n) {
                b();
                e(function () {
                    k.call(n)
                }, 1)
            }

            function lt(n, t) {
                t.code === r.INIT_ERROR ? n.destroy() : t.code === r.HTTP_ERROR && (t.file.status = r.FAILED, t.file.completeTimestamp = +new Date, nt(t.file), n.state == r.STARTED && (n.trigger("CancelUpload"), e(function () {
                    k.call(n)
                }, 1)))
            }

            function at(n) {
                n.stop();
                r.each(c, function (n) {
                    n.destroy()
                });
                c = [];
                a.length && (r.each(a, function (n) {
                    n.destroy()
                }), a = []);
                v.length && (r.each(v, function (n) {
                    n.destroy()
                }), v = []);
                p = {};
                y = !1;
                w = h = null;
                l.reset()
            }
            var g = r.guid()
                , u, c = []
                , p = {}
                , a = []
                , v = []
                , w, l, y = !1
                , h;
            u = {
                chunk_size: 0
                , file_data_name: "file"
                , filters: {
                    mime_types: []
                    , max_file_size: 0
                    , prevent_duplicates: !1
                    , prevent_empty: !0
                }
                , flash_swf_url: "js/Moxie.swf"
                , http_method: "POST"
                , max_retries: 0
                , multipart: !0
                , multi_selection: !0
                , resize: !1
                , runtimes: f.order
                , send_file_name: !0
                , send_chunk_number: !0
                , silverlight_xap_url: "js/Moxie.xap"
            };
            it.call(this, n, null, !0);
            l = new r.QueueProgress;
            r.extend(this, {
                id: g
                , uid: g
                , state: r.STOPPED
                , features: {}
                , runtime: null
                , files: c
                , settings: u
                , total: l
                , init: function () {
                    var n = this
                        , t, i;
                    if (t = n.getOption("preinit"), typeof t == "function" ? t(n) : r.each(t, function (t, i) {
                        n.bind(i, t)
                    }), rt.call(n), r.each(["container", "browse_button", "drop_element"], function (t) {
                        if (n.getOption(t) === null) return i = {
                            code: r.INIT_ERROR
                            , message: r.sprintf(r.translate("%s specified, but cannot be found."), t)
                        }, !1
                    }), i) return n.trigger("Error", i);
                    if (!u.browse_button && !u.drop_element) return n.trigger("Error", {
                        code: r.INIT_ERROR
                        , message: r.translate("You must specify either browse_button or drop_element.")
                    });
                    tt.call(n, u, function (t) {
                        var i = n.getOption("init")
                            , u;
                        typeof i == "function" ? i(n) : r.each(i, function (t, i) {
                            n.bind(i, t)
                        });
                        t ? (u = f.getInfo(d()), n.runtime = u.type, n.trigger("Init", {
                            runtime: u
                        }), n.trigger("PostInit")) : n.trigger("Error", {
                            code: r.INIT_ERROR
                            , message: r.translate("Init error.")
                        })
                    })
                }
                , setOption: function (n, t) {
                    it.call(this, n, t, !this.runtime)
                }
                , getOption: function (n) {
                    return n ? u[n] : u
                }
                , refresh: function () {
                    a.length && r.each(a, function (n) {
                        n.trigger("Refresh")
                    });
                    this.trigger("Refresh")
                }
                , start: function () {
                    this.state != r.STARTED && (this.state = r.STARTED, this.trigger("StateChanged"), k.call(this))
                }
                , stop: function () {
                    this.state != r.STOPPED && (this.state = r.STOPPED, this.trigger("StateChanged"), this.trigger("CancelUpload"))
                }
                , disableBrowse: function () {
                    y = arguments[0] !== i ? arguments[0] : !0;
                    a.length && r.each(a, function (n) {
                        n.disable(y)
                    });
                    this.trigger("DisableBrowse", y)
                }
                , getFile: function (n) {
                    for (var t = c.length - 1; t >= 0; t--)
                        if (c[t].id === n) return c[t]
                }
                , addFile: function (n, i) {
                    function a(n, t) {
                        var i = [];
                        r.each(u.settings.filters, function (t, r) {
                            o[r] && i.push(function (i) {
                                o[r].call(u, t, n, function (n) {
                                    i(!n)
                                })
                            })
                        });
                        r.inSeries(i, t)
                    }

                    function f(n) {
                        var o = r.typeOf(n);
                        if (n instanceof t.file.File) {
                            if (!n.ruid && !n.isDetached()) {
                                if (!s) return !1;
                                n.ruid = s;
                                n.connectRuntime(s)
                            }
                            f(new r.File(n))
                        } else n instanceof t.file.Blob ? (f(n.getSource()), n.destroy()) : n instanceof r.File ? (i && (n.name = i), h.push(function (t) {
                            a(n, function (i) {
                                i || (c.push(n), l.push(n), u.trigger("FileFiltered", n));
                                e(t, 1)
                            })
                        })) : r.inArray(o, ["file", "blob"]) !== -1 ? f(new t.file.File(null, n)) : o === "node" && r.typeOf(n.files) === "filelist" ? r.each(n.files, f) : o === "array" && (i = null, r.each(n, f))
                    }
                    var u = this
                        , h = []
                        , l = []
                        , s;
                    s = d();
                    f(n);
                    h.length && (u.trigger("StartResolveFiles"), r.inSeries(h, function () {
                        l.length && u.trigger("FilesAdded", l);
                        u.trigger("EndResolveFiles")
                    }))
                }
                , removeFile: function (n) {
                    for (var i = typeof n == "string" ? n : n.id, t = c.length - 1; t >= 0; t--)
                        if (c[t].id === i) return this.splice(t, 1)[0]
                }
                , splice: function (n, t) {
                    var u = c.splice(n === i ? 0 : n, t === i ? c.length : t)
                        , f = !1;
                    return this.state == r.STARTED && (r.each(u, function (n) {
                        if (n.status === r.UPLOADING) return f = !0, !1
                    }), f && this.stop()), this.trigger("FilesRemoved", u), r.each(u, function (n) {
                        n.destroy()
                    }), f && this.start(), u
                }
                , dispatchEvent: function (n) {
                    var t, r, i;
                    if (n = n.toLowerCase(), t = this.hasEventListener(n), t)
                        for (t.sort(function (n, t) {
                            return t.priority - n.priority
                        }), r = [].slice.call(arguments), r.shift(), r.unshift(this), i = 0; i < t.length; i++)
                            if (t[i].fn.apply(t[i].scope, r) === !1) return !1;
                    return !0
                }
                , bind: function (n, t, i, u) {
                    r.Uploader.prototype.bind.call(this, n, t, u, i)
                }
                , destroy: function () {
                    this.trigger("Destroy");
                    u = l = null;
                    this.unbindAll()
                }
            })
        };
        r.Uploader.prototype = t.core.EventTarget.instance;
        r.File = function () {
            function t(t) {
                r.extend(this, {
                    id: r.guid()
                    , name: t.name || t.fileName
                    , type: t.type || ""
                    , relativePath: t.relativePath || ""
                    , size: t.fileSize || t.size
                    , origSize: t.fileSize || t.size
                    , loaded: 0
                    , percent: 0
                    , status: r.QUEUED
                    , lastModifiedDate: t.lastModifiedDate || (new Date)
                        .toLocaleString()
                    , completeTimestamp: 0
                    , getNative: function () {
                        var n = this.getSource()
                            .getSource();
                        return r.inArray(r.typeOf(n), ["blob", "file"]) !== -1 ? n : null
                    }
                    , getSource: function () {
                        return n[this.id] ? n[this.id] : null
                    }
                    , destroy: function () {
                        var t = this.getSource();
                        t && (t.destroy(), delete n[this.id])
                    }
                });
                n[this.id] = t
            }
            var n = {};
            return t
        }();
        r.QueueProgress = function () {
            var n = this;
            n.size = 0;
            n.loaded = 0;
            n.uploaded = 0;
            n.failed = 0;
            n.queued = 0;
            n.percent = 0;
            n.bytesPerSec = 0;
            n.reset = function () {
                n.size = n.loaded = n.uploaded = n.failed = n.queued = n.percent = n.bytesPerSec = 0
            }
        };
        n.plupload = r
    })(this, n)
});
Ext.define("GleamTech.FileUltimate.FileUploader", {
    extend: "Ext.panel.Panel"
    , mixins: ["GleamTech.UI.ServerHandler", "GleamTech.UI.ActionHandler", "GleamTech.UI.EventHandler"]
    , statics: {
        sprite: new GleamTech.UI.Sprite("explorerviewicons")
        , sprite2: new GleamTech.UI.Sprite("filemanagericons")
    }
    , config: {
        resourceBasePath: ""
        , viewLayout: "details"
        , viewDetailsLayoutThreshold: 1e3
        , viewMultipleSelection: !0
        , viewCheckboxSelection: !1
        , showFileExtensions: !1
        , uploadHandlerType: "GleamTech.FileUltimate.PlUploadHandler"
        , uploadMethods: "Html5,Silverlight,Flash,Html4"
        , fileTypes: "*/"
        , maxFileSize: ""
        , chunkSize: "4 MB"
        , customParameters: null
        , autoStart: !1
        , closeAction: "hide"
        , autoClose: !1
    }
    , width: 600
    , height: 300
    , infoBar: null
    , multiView: null
    , statusBar: null
    , progressToolbar: null
    , bottomToolbar: null
    , messageBox: null
    , viewStore: null
    , queueStatusViewModel: null
    , viewSelection: null
    , contextMenuSelection: null
    , viewContainerContextMenu: null
    , viewItemContextMenu: null
    , uploadMethodMenu: null
    , busyManager: null
    , uploadHandler: null
    , uploadId: ""
    , allowedFileTypes: {
        items: []
        , regExp: null
        , text: ""
    }
    , deniedFileTypes: {
        items: []
        , regExp: null
        , text: ""
    }
    , constructor: function (n) {
        this.mixins.actionHandler.constructor.call(this, n);
        this.callParent(arguments)
    }
    , onDestroy: function () {
        this.callParent();
        Ext.destroy(this.viewStore, this.viewContainerContextMenu, this.viewItemContextMenu, this.uploadMethodMenu, this.uploadHandler)
    }
    , updateViewLayout: function (n) {
        this.multiView && this.multiView.setViewLayout(n)
    }
    , applyFileTypes: function (n, t) {
        var u, i, r;
        return (n = GleamTech.Util.String.trim(n || ""), n || (n = this.defaultConfig.fileTypes), n === t) ? t : (u = n.split("/"), i = this.allowedFileTypes, i.items = u[0].split("|"), Ext.Array.forEach(i.items, function (n, t, i) {
            i[t] = GleamTech.Util.String.trim(n)
        }), i.items = Ext.Array.filter(i.items, function (n) {
            return n !== ""
        }), i.items[0] === "*" ? (i.regExp = null, i.text = "") : (i.regExp = this.createFileTypesRegex(i.items), i.text = i.items.join(", ")), r = this.deniedFileTypes, r.items = u.length > 1 ? u[1].split("|") : [], Ext.Array.forEach(r.items, function (n, t, i) {
            i[t] = GleamTech.Util.String.trim(n)
        }), r.items = Ext.Array.filter(r.items, function (n) {
            return n !== ""
        }), r.items.length === 0 ? (r.regExp = null, r.text = "") : (r.regExp = this.createFileTypesRegex(r.items), r.text = r.items.join(", ")), i.items.join("|") + "/" + r.items.join("|"))
    }
    , updateFileTypes: function () {
        this.uploadHandler && this.uploadHandler.setMimeTypes(this.allowedFileTypes.items)
    }
    , createFileTypesRegex: function (n) {
        for (var r, u, t = "^(", i = 0; i < n.length; i++) r = GleamTech.Util.String.trim(n[i]), u = GleamTech.Util.String.escapeRegExpPattern(r), t += u.replace(/\\\?/g, ".")
            .replace(/\\\*/g, ".*"), i < n.length - 1 && (t += "|");
        return t += ")$", new RegExp(t, "i")
    }
    , updateMaxFileSize: function (n) {
        this.maxFileSizeInBytes = GleamTech.Util.Culture.parseByteSize(n)
    }
    , updateChunkSize: function (n) {
        this.chunkSizeInBytes = GleamTech.Util.Culture.parseByteSize(n);
        this.uploadHandler && this.uploadHandler.setChunkSizeInBytes(this.chunkSizeInBytes)
    }
    , initComponent: function () {
        this.viewSelection = [];
        this.viewCheckboxSelection || (this.viewCheckboxSelection = Ext.os.deviceType !== "Desktop");
        this.initLabels();
        this.initActions();
        this.initStores();
        var n = [];
        n.push(this.infoBar = this.createInfoBar());
        n.push(this.multiView = this.createMultiView());
        n.push(this.statusBar = this.createStatusBar(this.multiView));
        n.push(this.messageBox = this.createMessageBox());
        n.push({
            xtype: "container"
            , layout: "fit"
            , items: [this.progressToolbar = this.createProgressToolbar(), this.bottomToolbar = this.createBottomToolbar()]
        });
        Ext.apply(this, {
            layout: {
                type: "vbox"
                , align: "stretch"
            }
            , items: n
            , listeners: {
                beforeclose: this.onBeforeClose
                , beforehide: this.onBeforeHide
                , scope: this
            }
        });
        this.busyManager = new GleamTech.UI.BusyManager({
            listeners: {
                showBusy: {
                    fn: this.onShowBusy
                    , scope: this
                }
                , hideBusy: {
                    fn: this.onHideBusy
                    , scope: this
                }
            }
        });
        this.callParent();
        this.uploadMethodMenu = this.createUploadMethodMenu();
        this.viewStore.loadRecords([])
    }
    , afterRender: function () {
        this.callParent(arguments);
        this.uploadHandler = Ext.create(this.uploadHandlerType, {
            resourceBasePath: this.resourceBasePath
            , selectFileButton: this.bottomToolbar.query("button[itemId=AddFiles]")[0].el.dom
            , selectFolderButton: this.bottomToolbar.query("button[itemId=AddFolders]")[0].el.dom
            , uploadMethods: this.uploadMethods
            , chunkSizeInBytes: this.chunkSizeInBytes
            , mimeTypes: this.allowedFileTypes.items
            , listeners: {
                scope: this
                , load: this.onUploadHandlerLoad
                , loaderror: this.onUploadHandlerLoadError
                , unload: this.onUploadHandlerUnload
                , addfiles: this.onUploadHandlerAddFiles
                , startbusy: this.onUploadHandlerStartBusy
                , endbusy: this.onUploadHandlerEndBusy
                , fileprogress: this.onUploadHandlerFileProgress
                , fileend: this.onUploadHandlerFileEnd
            }
        })
    }
    , onShow: function () {
        this.callParent(arguments);
        this.uploadHandler && this.uploadHandler.uploadMethod && this.fireEventEx("ready", new GleamTech.UI.EventArgs({
            uploadMethod: this.uploadHandler.uploadMethod
            , uploadMethodFeatures: this.uploadHandler.uploadMethodFeatures
        }));
        GleamTech.Util.Dom.registerPageLeave(this.onPageLeave, this)
    }
    , onBeforeClose: function () {
        return this.isClosing = this.onBeforeLeave("close"), this.isClosing
    }
    , onBeforeHide: function () {
        return this.isDestroying ? !0 : this.isClosing ? (delete this.isClosing, !0) : this.onBeforeLeave("hide")
    }
    , onBeforeLeave: function (n) {
        return this.queueStatusViewModel.data.inProgress ? (this.messageBox.showConfirm({
            message: GleamTech.Util.Language.getEntry("Message.Confirm.UploadStillInProgress")
            , fn: function (t) {
                t === "yes" && this[n]()
            }
            , scope: this
        }), !1) : !0
    }
    , onPageLeave: function (n) {
        this.queueStatusViewModel.data.inProgress && (n.message = GleamTech.Util.Language.getEntry("Message.Confirm.UploadStillInProgress"))
    }
    , onHide: function () {
        this.stop();
        this.onActionClear();
        this.multiView.setViewLayout(this.viewLayout);
        GleamTech.Util.Dom.unregisterPageLeave(this.onPageLeave, this);
        this.callParent(arguments)
    }
    , initLabels: function () {
        this.emptyViewText = GleamTech.Util.Language.getEntry("Message.DragAndDrop")
    }
    , initActions: function () {
        this.addControlActions([{
            actionName: "SelectAll"
            , languageKey: "Label.SelectAll"
            , iconName: "SelectAll"
        }, {
            actionName: "SelectNone"
            , languageKey: "Label.SelectNone"
            , iconName: "SelectNone"
        }, {
            actionName: "InvertSelection"
            , languageKey: "Label.InvertSelection"
            , iconName: "InvertSelection"
        }, {
            actionName: "ToggleLayoutExtraLargeIcons"
            , languageKey: "Label.Layout.ExtraLargeIcons"
            , iconName: "ExtraLargeIcons"
        }, {
            actionName: "ToggleLayoutLargeIcons"
            , languageKey: "Label.Layout.LargeIcons"
            , iconName: "LargeIcons"
        }, {
            actionName: "ToggleLayoutMediumIcons"
            , languageKey: "Label.Layout.MediumIcons"
            , iconName: "MediumIcons"
        }, {
            actionName: "ToggleLayoutSmallIcons"
            , languageKey: "Label.Layout.SmallIcons"
            , iconName: "SmallIcons"
        }, {
            actionName: "ToggleLayoutList"
            , languageKey: "Label.Layout.List"
            , iconName: "List"
        }, {
            actionName: "ToggleLayoutDetails"
            , languageKey: "Label.Layout.Details"
            , iconName: "Details"
        }, {
            actionName: "ToggleLayoutTiles"
            , languageKey: "Label.Layout.Tiles"
            , iconName: "Tiles"
        }, {
            actionName: "ToggleLayoutContent"
            , languageKey: "Label.Layout.Content"
            , iconName: "Content"
        }, {
            actionName: "ToggleItemCheckBoxes"
            , languageKey: "Label.ItemCheckBoxes"
        }, {
            actionName: "AddFiles"
            , languageKey: "Label.AddFiles"
        }, {
            actionName: "AddFolders"
            , languageKey: "Label.AddFolders"
        }, {
            actionName: "Remove"
            , languageKey: "Label.Remove"
        }, {
            actionName: "Clear"
            , languageKey: "Label.Clear"
        }, {
            actionName: "UploadMethod"
            , languageKey: "Label.UploadMethod"
        }, {
            actionName: "ViewInfo"
            , languageKey: "Label.ViewInfo"
        }, {
            actionName: "Upload"
            , languageKey: "Label.Upload.Verb"
        }, {
            actionName: "Cancel"
            , languageKey: "Label.Cancel"
        }, {
            actionName: "CancelAll"
            , languageKey: "Label.CancelAll"
        }], this.statics()
            .sprite)
    }
    , initStores: function () {
        this.viewStore = new GleamTech.FileUltimate.UploadStore({
            model: "GleamTech.FileUltimate.UploadModel"
            , listeners: {
                scope: this
                , datachanged: this.onViewStoreDataChanged
            }
        });
        this.queueStatusViewModel = new GleamTech.FileUltimate.QueueStatusViewModel
    }
    , createInfoBar: function () {
        var n = new Ext.toolbar.Toolbar({
            border: 0
            , cls: "x-statusbar"
            , layout: "hbox"
            , padding: "6 6 6 6"
            , items: [new Ext.Component({
                width: 32
                , height: 32
                , margin: "0 6 0 0"
                , cls: this.statics()
                    .sprite2.getIcon("Upload")
                    .getIconCls(32)
            }), this.infoBarTitle = new Ext.Component({
                cls: "x-infobar-title"
                , html: GleamTech.Util.Language.getEntry("Message.SelectToUpload")
            })]
        });
        return Crypto.MD5(this.id + this.stateId) !== this.hash && n.add(["->", {
            xtype: "component"
            , autoEl: {
                tag: "a"
                , href: "https://www.gleamtech.com/fileultimate/buy"
                , target: "_blank"
                , html: "FileUltimate (unlicensed) - Purchase a license"
                , style: {
                    color: "#000080"
                }
            }
        }]), n
    }
    , createMultiView: function () {
        return new GleamTech.UI.MultiView({
            flex: 1
            , store: this.viewStore
            , columns: this.getViewColumnsConfig()
            , viewLayout: this.viewLayout
            , detailsLayoutThreshold: this.viewDetailsLayoutThreshold
            , multipleSelection: this.viewMultipleSelection
            , checkboxSelection: this.viewCheckboxSelection
            , getItemIconCls: {
                fn: this.getItemIconCls
                , scope: this
            }
            , getItemThumbnailSrc: {
                fn: this.getItemThumbnailSrc
                , scope: this
            }
            , getItemDynamicThumbnailSrc: {
                fn: this.getItemDynamicThumbnailSrc
                , scope: this
            }
            , enableNativeDrop: !0
            , emptyCls: "x-grid-empty x-grid-empty-middle"
            , listeners: {
                scope: this
                , containercontextmenu: this.onViewContainerContextMenu
                , itemcontextmenu: this.onViewItemContextMenu
                , selectionchange: this.onViewSelectionChange
                , layoutchange: this.onViewLayoutChange
                , layoutstatechange: this.onViewLayoutStateChange
                , nativedragenter: this.onViewNativeDragEnter
                , nativedrop: this.onViewNativeDrop
            }
        })
    }
    , getViewColumnsConfig: function (n) {
        var u = this
            , f = {
                name: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Name")
                    , dataIndex: "name"
                    , formatterFn: function (n, t) {
                        return !u.showFileExtensions && t.data.itemType === GleamTech.FileUltimate.UploadItemType.File ? GleamTech.Util.Path.getFileNameWithoutExtension(n) : n
                    }
                    , hideable: !1
                    , isPrimary: !0
                    , width: 272
                }
                , dateModified: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.DateModified")
                    , dataIndex: "dateModified"
                    , formatterFn: function (n) {
                        return GleamTech.Util.Culture.formatShortDateTime(n)
                    }
                    , isToolTipValue: !0
                    , width: 125
                }
                , type: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Type")
                    , dataIndex: "type"
                    , isTileFirstValue: !0
                    , tileFormatterFn: function (n, t) {
                        return t.data.itemType === GleamTech.FileUltimate.UploadItemType.File ? n : ""
                    }
                    , isToolTipValue: !0
                    , width: 125
                }
                , size: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Size")
                    , dataIndex: "size"
                    , align: "right"
                    , formatterFn: function (n) {
                        return GleamTech.Util.Culture.formatKBSize(n)
                    }
                    , isTileSecondValue: !0
                    , tileFormatterFn: function (n, t) {
                        return t.data.itemType === GleamTech.FileUltimate.UploadItemType.File ? GleamTech.Util.Culture.formatByteSize(n) : ""
                    }
                    , isToolTipValue: !0
                    , width: 85
                }
                , status: {
                    text: GleamTech.Util.Language.getEntry("Label.Column.Status")
                    , width: 100
                    , xtype: "widgetcolumn"
                    , dataIndex: "status"
                    , sortable: !0
                    , widget: {
                        xtype: "progresswithstatuswidget"
                        , bind: "{record.status}"
                        , textTpl: '{percent:number("0")}%'
                        , showDetails: function (n) {
                            u.messageBox.showError(n.details)
                        }
                    }
                }
            }
            , i, t, r;
        if (n) {
            for (i = [], t = 0; t < n.length; t++) r = f[n[t]], r && i.push(r);
            return i
        }
        return Ext.Object.getValues(f)
    }
    , getItemIconCls: function (n, t) {
        var i = n.data;
        switch (i.itemType) {
            case GleamTech.FileUltimate.UploadItemType.Folder:
                return GleamTech.FileUltimate.FileTypeInfo.getBaseIconCls("Folder", t);
            case GleamTech.FileUltimate.UploadItemType.File:
                return GleamTech.FileUltimate.FileTypeInfo.getIconCls(i.extension, t);
            default:
                return ""
        }
    }
    , getItemThumbnailSrc: function (n) {
        if (!this.uploadHandler.uploadMethodFeatures.displayThumbnails) return "";
        var t = n.data;
        return t.itemType !== GleamTech.FileUltimate.UploadItemType.File || t.size === 0 ? "" : this.uploadHandler.isThumbnailExtension(t.extension) ? "{dynamic}" : ""
    }
    , getItemDynamicThumbnailSrc: function (n, t, i, r) {
        var f = n.data.thumbnails
            , u;
        if (f && (u = f[t], u)) {
            i(u);
            return
        }
        this.uploadHandler.getThumbnail(n, t, function (r) {
            var u = n.data.thumbnails;
            u || (u = n.data.thumbnails = {});
            u[t] = r;
            i(r)
        }, r)
    }
    , createStatusBar: function (n) {
        return new GleamTech.UI.MultiViewStatusBar({
            border: 0
            , multiView: n
            , getDataStatusText: {
                fn: this.getTotalSize
                , scope: this
            }
            , getSelectionStatusText: {
                fn: this.getTotalSize
                , scope: this
            }
        })
    }
    , getTotalSize: function (n) {
        var i, r, t, u;
        if (this.uploadHandler && this.uploadHandler.uploadMethod && this.uploadHandler.uploadMethodFeatures !== null && !this.uploadHandler.uploadMethodFeatures.reportFileSize || (i = 0, r = n.length, r === 0)) return "";
        for (t = 0; t < r; t++) u = n[t], u.data.itemType === GleamTech.FileUltimate.UploadItemType.File && (i += u.data.size);
        return GleamTech.Util.Culture.formatByteSize(i)
    }
    , createProgressToolbar: function () {
        var n = this;
        return new Ext.toolbar.Toolbar({
            ui: "footer"
            , hidden: !0
            , layout: {
                type: "vbox"
                , align: "stretch"
            }
            , viewModel: this.queueStatusViewModel
            , items: [{
                xtype: "displayfield"
                , fieldLabel: GleamTech.Util.Language.getEntry("Label.TimeRemaining")
                , bind: "{timeRemaining}"
            }, {
                xtype: "displayfield"
                , fieldLabel: GleamTech.Util.Language.getEntry("Label.ItemsRemaining")
                , bind: "{itemsRemaining}"
            }, {
                xtype: "displayfield"
                , fieldLabel: GleamTech.Util.Language.getEntry("Label.Speed")
                , bind: "{speed}"
            }, {
                xtype: "toolbar"
                , ui: "footer"
                , padding: 0
                , margin: 0
                , defaults: {
                    minWidth: 80
                }
                , items: [{
                    xtype: "progressbar"
                    , flex: 1
                    , bind: "{progress}"
                }, this.applyControlAction({
                    _enableConditionFn: function () {
                        return n.viewStore.getCount() > 0 && n.uploadHandler !== null && n.uploadHandler.uploadMethod !== null
                    }
                }, "CancelAll")]
            }]
        })
    }
    , createBottomToolbar: function () {
        var n = this;
        return new Ext.toolbar.Toolbar({
            ui: "footer"
            , defaults: {
                minWidth: 80
            }
            , items: [this.applyControlAction({
                enableConditionFn: function () {
                    return n.uploadHandler !== null && n.uploadHandler.uploadMethod !== null && n.uploadHandler.uploadMethodFeatures !== null && n.uploadHandler.uploadMethodFeatures.selectFile
                }
            }, "AddFiles"), this.applyControlAction({
                enableConditionFn: function () {
                    return n.uploadHandler !== null && n.uploadHandler.uploadMethod !== null && n.uploadHandler.uploadMethodFeatures !== null && n.uploadHandler.uploadMethodFeatures.selectFolder
                }
            }, "AddFolders"), this.applyControlAction({
                enableConditionFn: function () {
                    return n.viewSelection.length > 0
                }
            }, "Remove"), this.applyControlAction({
                enableConditionFn: function () {
                    return n.viewStore.getCount() > 0
                }
            }, "Clear"), "->", this.applyControlAction({
                enableConditionFn: function () {
                    return n.uploadHandler !== null && n.uploadHandler.uploadMethod !== null && n.viewStore.hasAnyPending()
                }
            }, "Upload")]
        })
    }
    , createMessageBox: function () {
        return new GleamTech.UI.MessageBox({
            buttonText: {
                ok: GleamTech.Util.Language.getEntry("Label.OK")
                , yes: GleamTech.Util.Language.getEntry("Label.Yes")
                , no: GleamTech.Util.Language.getEntry("Label.No")
                , cancel: GleamTech.Util.Language.getEntry("Label.Cancel")
            }
            , conflictText: {
                message: GleamTech.Util.Language.getEntry("Message.FileConflict")
                , actionMessage: GleamTech.Util.Language.getEntry("Message.FileConflictAction")
                , replaceTitle: GleamTech.Util.Language.getEntry("Label.FileConflictReplace")
                , replaceDescription: GleamTech.Util.Language.getEntry("Message.FileConflictReplace")
                , skipTitle: GleamTech.Util.Language.getEntry("Label.FileConflictSkip")
                , skipDescription: GleamTech.Util.Language.getEntry("Message.FileConflictSkip")
                , keepBothTitle: GleamTech.Util.Language.getEntry("Label.FileConflictKeepBoth")
                , keepBothDescription: GleamTech.Util.Language.getEntry("Message.FileConflictKeepBoth")
                , nextConflicts: GleamTech.Util.Language.getEntry("Message.FileConflictSameForNext")
                , allConflicts: GleamTech.Util.Language.getEntry("Message.FileConflictSameForAll")
            }
        })
    }
    , createViewContainerContextMenu: function () {
        var n = this;
        return new Ext.menu.Menu({
            items: [this.applyControlAction({
                enableConditionFn: function () {
                    return n.viewStore.getCount() > 0
                }
                , showConditionFn: function () {
                    return !n.queueStatusViewModel.data.inProgress
                }
            }, "Clear", 16), this.applyControlAction({
                showConditionFn: function () {
                    return n.queueStatusViewModel.data.inProgress
                }
            }, "CancelAll", 16), {
                xtype: "menuseparator"
            }, this.applyControlAction({
                showConditionFn: function () {
                    return !n.queueStatusViewModel.data.inProgress
                }
                , menu: this.uploadMethodMenu
            }, "UploadMethod", 16), {
                xtype: "menuseparator"
            }, this.applyControlAction({}, "SelectAll", 16)]
            , listeners: {
                scope: this
                , beforeshow: this.onViewContainerContextMenuBeforeShow
            }
        })
    }
    , createUploadMethodMenu: function () {
        for (var i, u, r = Ext.ClassManager.get(this.uploadHandlerType)
            .supportedUploadMethods, n = [], t = 0; t < r.length; t++) i = r[t], u = this.applyControlAction({
                itemId: "UploadMethod" + i
                , text: i
                , group: "UploadMethod"
                , checked: !1
            }, "UploadMethod"), n.push(u);
        return n.push({
            xtype: "menuseparator"
        }), n.push(this.applyControlAction({}, "ViewInfo", 16)), new Ext.menu.Menu({
            items: n
        })
    }
    , createViewItemContextMenu: function () {
        var n = this;
        return new Ext.menu.Menu({
            items: [this.applyControlAction({
                enableConditionFn: function () {
                    return n.viewSelection.length > 0
                }
                , showConditionFn: function () {
                    return !n.queueStatusViewModel.data.inProgress
                }
            }, "Remove", 16), this.applyControlAction({
                showConditionFn: function () {
                    return n.queueStatusViewModel.data.inProgress
                }
            }, "Cancel", 16), {
                xtype: "menuseparator"
            }, this.applyControlAction({}, "SelectAll", 16), this.applyControlAction({}, "SelectNone", 16), this.applyControlAction({}, "InvertSelection", 16)]
            , listeners: {
                scope: this
                , beforeshow: this.onViewItemContextMenuBeforeShow
            }
        })
    }
    , onViewStoreDataChanged: function () {
        this.queueStatusViewModel.data.inProgress || this.bottomToolbar.toggleItemsUICondition(this.uiConditionFn)
    }
    , onViewLayoutChange: function (n, t, i) {
        this.getControlAction("ToggleLayout" + t.name)
            .each(function (n) {
                n.toggleUIOnly(!0)
            });
        i && this.getControlAction("ToggleLayout" + i.name)
            .each(function (n) {
                n.toggleUIOnly(!1)
            })
    }
    , onViewLayoutStateChange: function (n, t) {
        for (var r, u, i = 0; i < t.length; i++) r = t[i], u = this.getControlAction("ToggleLayout" + r.name), r.enabled ? u.enable() : u.disable()
    }
    , onViewContainerContextMenu: function (n, t) {
        this.viewContainerContextMenu || (this.viewContainerContextMenu = this.createViewContainerContextMenu());
        this.showContextMenu(t, this.viewContainerContextMenu)
    }
    , onViewItemContextMenu: function (n, t, i, r, u) {
        this.viewItemContextMenu || (this.viewItemContextMenu = this.createViewItemContextMenu());
        this.showContextMenu(u, this.viewItemContextMenu, t)
    }
    , onViewContainerContextMenuBeforeShow: function (n) {
        n.toggleItemsUICondition(this.uiConditionFn)
    }
    , onViewItemContextMenuBeforeShow: function (n) {
        n.toggleItemsUICondition(this.uiConditionFn)
    }
    , onViewNativeDragEnter: function (n, t) {
        if (this.queueStatusViewModel.data.inProgress) {
            t.proxy.setStatusNotAllowed();
            return
        }
        var i = this.uploadHandler && this.uploadHandler.uploadMethod && this.uploadHandler.uploadMethodFeatures && this.uploadHandler.uploadMethodFeatures.dragAndDrop;
        i && t.hasFiles ? t.proxy.setStatusText(GleamTech.Util.Language.getEntry("Label.AddToUploadQueue"), "x-dd-status-uploadto") : t.proxy.setStatusNotAllowed()
    }
    , onViewNativeDrop: function (n, t) {
        var r, i;
        this.queueStatusViewModel.data.inProgress || (r = this.uploadHandler && this.uploadHandler.uploadMethod && this.uploadHandler.uploadMethodFeatures && this.uploadHandler.uploadMethodFeatures.dragAndDrop, r && t.hasFiles) && (i = this, GleamTech.FileUltimate.FileDropHelper.getFilesFromEvent(t.e.browserEvent, function (n) {
            i.uploadHandler.addFiles(n)
        }, function (n) {
            i.messageBox.showWarning(n)
        }))
    }
    , onShowBusy: function () {
        this.setLoading(!0)
    }
    , onHideBusy: function () {
        this.setLoading(!1)
    }
    , showContextMenu: function (n, t, i) {
        n.stopEvent();
        this.contextMenuSelection = i;
        t.showAt(n.getXY())
    }
    , onViewSelectionChange: function (n, t) {
        this.viewSelection = t;
        this.queueStatusViewModel.data.inProgress || this.bottomToolbar.toggleItemsUICondition(this.uiConditionFn)
    }
    , uiConditionFn: function (n, t) {
        var i = n[t ? "showConditionFn" : "enableConditionFn"];
        return Ext.isFunction(i) ? Ext.callback(i) : undefined
    }
    , showAddConflicts: function (n, t) {
        var i = n.shift()
            , r;
        if (!i) {
            t && t();
            return
        }
        r = this;
        this.messageBox.showWarning({
            title: GleamTech.Util.Language.getEntry("Label.AddConflict")
            , message: i.message
            , fn: function () {
                r.showAddConflicts(n, t)
            }
            , itemsConfig: {
                records: i.records
                , model: "GleamTech.FileUltimate.UploadModel"
                , multiViewConfig: {
                    viewLayout: this.multiView.viewLayout.name
                    , detailsLayoutThreshold: this.viewDetailsLayoutThreshold
                    , columns: this.getViewColumnsConfig(["name", "dateModified", "type", "size"])
                    , getItemIconCls: {
                        fn: this.getItemIconCls
                        , scope: this
                    }
                    , getItemThumbnailSrc: {
                        fn: this.getItemThumbnailSrc
                        , scope: this
                    }
                    , getItemDynamicThumbnailSrc: {
                        fn: this.getItemDynamicThumbnailSrc
                        , scope: this
                    }
                }
            }
        })
    }
    , showUploadConflicts: function (n, t, i, r) {
        var u = n.shift()
            , f, e;
        if (!u) {
            t && t();
            return
        }
        f = this;
        e = function (i, r) {
            switch (i) {
                case "skip":
                    u.newRecord.set("status", f.getStatusData(GleamTech.FileUltimate.UploadStatus.Skipped));
                    break;
                case "keepboth":
                    u.newRecord.set("name", u.itemCopyName);
                    break;
                case "cancel":
                    f.stop();
                    return
            }
            f.showUploadConflicts(n, t, i, r)
        };
        r ? e(i, r) : this.messageBox.showConflict({
            title: GleamTech.Util.Language.getEntry("Label.UploadConflict")
            , conflictConfig: {
                newRecord: u.newRecord
                , existingRecord: u.existingRecord
                , itemCopyName: u.itemCopyName
                , columns: this.getViewColumnsConfig(["name", "size", "dateModified"])
                , getItemIconCls: {
                    fn: this.getItemIconCls
                    , scope: this
                }
                , remainingConflicts: n.length
            }
            , fn: function (n, t, i) {
                e(n, i.doSameChecked)
            }
        })
    }
    , getStatusData: function (n, t, i) {
        var r = {
            value: n
            , details: t
        };
        switch (n) {
            case GleamTech.FileUltimate.UploadStatus.Pending:
                Ext.apply(r, {
                    text: GleamTech.Util.Language.getEntry("Label.Status.Pending")
                });
                break;
            case GleamTech.FileUltimate.UploadStatus.Rejected:
                Ext.apply(r, {
                    text: GleamTech.Util.Language.getEntry("Label.Status.Rejected")
                    , color: "red"
                });
                break;
            case GleamTech.FileUltimate.UploadStatus.Skipped:
                Ext.apply(r, {
                    text: GleamTech.Util.Language.getEntry("Label.Status.Skipped")
                    , color: "navy"
                });
                break;
            case GleamTech.FileUltimate.UploadStatus.Uploading:
                Ext.apply(r, {
                    progress: i
                });
                break;
            case GleamTech.FileUltimate.UploadStatus.Canceled:
                Ext.apply(r, {
                    text: GleamTech.Util.Language.getEntry("Label.Status.Canceled")
                    , color: "orange"
                });
                break;
            case GleamTech.FileUltimate.UploadStatus.Failed:
                Ext.apply(r, {
                    text: GleamTech.Util.Language.getEntry("Label.Status.Failed")
                    , color: "red"
                });
                break;
            case GleamTech.FileUltimate.UploadStatus.Completed:
                Ext.apply(r, {
                    text: GleamTech.Util.Language.getEntry("Label.Status.Completed")
                    , color: "green"
                })
        }
        return r
    }
    , onQueueStatusChange: function (n, t) {
        switch (n) {
            case GleamTech.FileUltimate.QueueStatus.Starting:
                this.queueStatusViewModel.set({
                    inProgress: !0
                    , uploadId: GleamTech.Util.String.createUniqueId()
                });
                this.infoBarTitle.setHtml(GleamTech.Util.Language.getEntry("Message.UploadInProgress"));
                this.progressToolbar.show();
                this.multiView.lockViewLayout("details");
                this.viewStore.sort({
                    property: "internalId"
                    , direction: "ASC"
                    , root: ""
                });
                break;
            case GleamTech.FileUltimate.QueueStatus.Started:
                this.queueStatusViewModel.set({
                    totalCount: t.totalCount
                    , totalSize: t.totalSize
                });
                break;
            case GleamTech.FileUltimate.QueueStatus.FileStarted:
                this.queueStatusViewModel.data.processedCount === 0 && this.queueStatusViewModel.set({
                    startTime: (new Date)
                        .getTime()
                    , fileProcessedSize: 0
                    , fileProgress: 0
                });
                break;
            case GleamTech.FileUltimate.QueueStatus.FileProcessing:
                this.queueStatusViewModel.set({
                    fileProcessedSize: t.fileProcessedSize
                    , fileProgress: t.fileProgress
                });
                break;
            case GleamTech.FileUltimate.QueueStatus.FileStopped:
                this.queueStatusViewModel.set({
                    processedSize: this.queueStatusViewModel.get("processedSize") + this.queueStatusViewModel.get("fileProcessedSize")
                    , processedCount: this.queueStatusViewModel.get("processedCount") + 1
                    , fileProcessedSize: 0
                    , fileProgress: 0
                });
                break;
            case GleamTech.FileUltimate.QueueStatus.Stopped:
                this.queueStatusViewModel.setData(this.queueStatusViewModel.defaultConfig.data);
                this.infoBarTitle.setHtml(GleamTech.Util.Language.getEntry("Message.SelectToUpload"));
                this.progressToolbar.hide();
                this.multiView.unlockViewLayout();
                this.bottomToolbar.toggleItemsUICondition(this.uiConditionFn)
        }
    }
    , getEventItems: function (n) {
        for (var t, r = [], i = 0; i < n.length; i++) t = n[i], r.push({
            itemType: Ext.Object.getKey(GleamTech.FileUltimate.UploadItemType, t.data.itemType)
            , name: t.data.name
            , nameWithoutPath: t.data.nameWithoutPath
            , extension: t.data.extension
            , dateModified: t.data.dateModified
            , type: t.data.type
            , size: t.data.size
            , status: Ext.Object.getKey(GleamTech.FileUltimate.UploadStatus, t.data.status.value)
        });
        return r
    }
    , onCancelEvent: function (n) {
        n.cancelMessage && this.messageBox.showError({
            title: GleamTech.Util.Language.getEntry("Label.ActionCanceled")
            , message: n.cancelMessage
        })
    }
    , addFiles: function (n, t) {
        this.uploadHandler && (this.autoStartOnce = t, this.uploadHandler.addFiles(n))
    }
    , start: function () {
        this.queueStatusViewModel.data.inProgress || this.onBeginQueue()
    }
    , onBeginQueue: function () {
        var i, u, r, t, n, f;
        this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Starting);
        for (i = this.viewStore.getData()
            .items, this.queuedRecords = [], u = this.uploadHandler.uploadMethodFeatures.reportFileSize, r = {}, t = 0; t < i.length; t++)(n = i[t], n.data.status.value === GleamTech.FileUltimate.UploadStatus.Pending) && (this.queuedRecords.push(n), r[n.data.id] = {
                Name: n.data.name
                , Size: u ? n.data.size : null
            });
        if (!this.queuedRecords.length) {
            this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Stopped);
            return
        }
        if (f = this.fireEventEx("uploading", new GleamTech.UI.EventArgs({
            items: this.getEventItems(this.queuedRecords)
            , uploadMethod: this.uploadHandler.uploadMethod
            , uploadMethodFeatures: this.uploadHandler.uploadMethodFeatures
        })), f === !1) {
            this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Stopped);
            return
        }
        this.callServerHandlerMethod({
            name: "Begin"
            , parameters: {
                uploadId: this.queueStatusViewModel.data.uploadId
                , method: this.uploadHandler.uploadMethod
                , validations: r
                , customParameters: this.customParameters || {}
            }
            , callback: function (n, t) {
                var f, u, r, i, e;
                if (t) {
                    this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Stopped);
                    return
                }
                for (f = [], u = 0; u < this.queuedRecords.length; u++) {
                    r = this.queuedRecords[u];
                    i = n[r.data.id];
                    switch (i.Action) {
                        case GleamTech.FileUltimate.ValidationAction.Accept:
                            continue;
                        case GleamTech.FileUltimate.ValidationAction.Reject:
                            r.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Rejected, i.ActionData));
                            continue;
                        case GleamTech.FileUltimate.ValidationAction.ConfirmReplace:
                            f.push({
                                newRecord: r
                                , existingRecord: new GleamTech.FileUltimate.UploadModel({
                                    itemType: r.data.itemType
                                    , name: i.ActionData.ExistingFileName
                                    , size: i.ActionData.ExistingFileSize
                                    , dateModified: i.ActionData.ExistingFileDate
                                })
                                , itemCopyName: i.ActionData.NewFileName
                            })
                    }
                }
                e = this;
                this.showUploadConflicts(f, function () {
                    e.onConflictsResolved()
                })
            }
            , scope: this
        })
    }
    , onConflictsResolved: function () {
        for (var n, t = [], i = [], u = this.uploadHandler.uploadMethodFeatures.reportFileSize, f = u ? 0 : null, r = 0; r < this.queuedRecords.length; r++) n = this.queuedRecords[r], n.data.status.value === GleamTech.FileUltimate.UploadStatus.Pending ? (u && n.data.itemType === GleamTech.FileUltimate.UploadItemType.File && (f += n.data.size), i.push(n)) : n.data.status.value === GleamTech.FileUltimate.UploadStatus.Skipped && t.push(n.data.id);
        if (t.length) this.onSkip(t);
        if (!i.length) {
            this.onEndQueue();
            return
        }
        this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Started, {
            totalCount: i.length
            , totalSize: f
        });
        this.onUploadNext()
    }
    , onSkip: function (n) {
        this.callServerHandlerMethod({
            name: "Skip"
            , parameters: {
                uploadId: this.queueStatusViewModel.data.uploadId
                , itemIds: n
            }
        })
    }
    , onUploadNext: function () {
        var n, t;
        if (this.queueStatusViewModel.data.inProgress) {
            for (n = 0; n < this.queuedRecords.length; n++) {
                if (t = this.queuedRecords[n], t.data.status.value === GleamTech.FileUltimate.UploadStatus.Uploading) return;
                if (t.data.status.value === GleamTech.FileUltimate.UploadStatus.Pending) {
                    this.onUpload(t);
                    return
                }
            }
            this.onEndQueue()
        }
    }
    , onUpload: function (n) {
        var i = this.fireEventEx("itemuploading", new GleamTech.UI.EventArgs({
            item: this.getEventItems([n])[0]
            , uploadMethod: this.uploadHandler.uploadMethod
            , uploadMethodFeatures: this.uploadHandler.uploadMethodFeatures
        }))
            , t;
        if (i === !1) {
            n.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Skipped));
            this.onSkip([n.data.id]);
            return
        }
        var r = this.uploadHandler.uploadMethodFeatures.reportFileSize
            , u = this.uploadHandler.uploadMethodFeatures.reportProgress
            , f = this.uploadHandler.uploadMethodFeatures.sendAsChunks
            , e = this.getServerHandlerMethodUrl(f ? "SendStream" : "SendMultipart", {
                stateId: this.stateId
                , uploadId: this.queueStatusViewModel.data.uploadId
                , itemId: n.data.id
                , size: r ? n.data.size : null
                , name: n.data.name
                , dateModified: Ext.Date.format(n.data.dateModified, "C")
            });
        this.multiView.activeView.focusNode(n);
        n.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Uploading, null, 0));
        this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.FileStarted);
        this.uploadHandler.uploadFile(n, e);
        t = this;
        u ? this.queueStatusViewModel.data.processedCount === 0 && function s() {
            setTimeout(function () {
                t.callServerHandlerMethod({
                    name: "KeepSessionAlive"
                    , callback: function (n, i) {
                        i || t.queueStatusViewModel.data.inProgress && s()
                    }
                })
            }, 3e4)
        }() : function o(i, r) {
            setTimeout(function () {
                n.destroyed || t.callServerHandlerMethod({
                    name: "GetProgress"
                    , parameters: {
                        uploadId: t.queueStatusViewModel.data.uploadId
                        , itemId: n.data.id
                        , first: r
                    }
                    , callback: function (i, r) {
                        if (!r && i !== null && !n.destroyed && n.data.status.value === GleamTech.FileUltimate.UploadStatus.Uploading) {
                            var u = i[0]
                                , f = i[1];
                            n.data.size === null && n.set("size", u);
                            t.onUploadProgress(n, f);
                            o(1e3, !1)
                        }
                    }
                })
            }, i)
        }(0, !0)
    }
    , onFail: function (n, t) {
        n.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Failed, t));
        this.callServerHandlerMethod({
            name: "Fail"
            , parameters: {
                uploadId: this.queueStatusViewModel.data.uploadId
                , itemId: n.data.id
                , clientError: Ext.encode(t)
            }
        })
    }
    , onCancel: function (n, t) {
        for (var i, u = [], r = 0; r < n.length; r++)
            if (i = n[r], i.data.status.value === GleamTech.FileUltimate.UploadStatus.Uploading || i.data.status.value === GleamTech.FileUltimate.UploadStatus.Pending) {
                this.uploadHandler.cancelFile(i);
                i.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Canceled));
                this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.FileProcessing, {
                    fileProcessedSize: i.data.size || 0
                    , fileProgress: 1
                });
                this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.FileStopped);
                u.push(i.data.id)
            } this.callServerHandlerMethod({
                name: "Cancel"
                , parameters: {
                    uploadId: this.queueStatusViewModel.data.uploadId
                    , itemIds: u
                }
                , callback: function () {
                    t || this.onUploadNext()
                }
                , scope: this
            })
    }
    , onEndQueue: function () {
        var t, n, i;
        this.callServerHandlerMethod({
            name: "End"
            , parameters: {
                uploadId: this.queueStatusViewModel.data.uploadId
            }
        });
        this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Stopped);
        if (this.fireEventEx("uploaded", new GleamTech.UI.EventArgs({
            items: this.getEventItems(this.queuedRecords)
            , uploadMethod: this.uploadHandler.uploadMethod
            , uploadMethodFeatures: this.uploadHandler.uploadMethodFeatures
        })), this.autoClose) {
            for (t = !0, n = 0; n < this.queuedRecords.length; n++)
                if (i = this.queuedRecords[n], i.data.status.value !== GleamTech.FileUltimate.UploadStatus.Completed) {
                    t = !1;
                    break
                } t && this.close()
        }
        this.queuedRecords.length = 0
    }
    , stop: function () {
        if (this.queueStatusViewModel.data.inProgress) {
            this.onCancel(this.queuedRecords, !0);
            this.onEndQueue()
        }
    }
    , onServerHandlerMethodEnd: function (n, t, i) {
        if (i)
            if (i.sessionExpired) {
                this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.Stopped);
                this.messageBox.showConfirm({
                    title: GleamTech.Util.Language.getEntry("Label.RefreshPage")
                    , message: GleamTech.Util.Language.getEntry("Message.SessionExpiredRefresh", 10)
                    , fn: function (n) {
                        n === "ok" && GleamTech.Util.Dom.refreshPage()
                    }
                    , buttons: Ext.Msg.OKCANCEL
                    , countDownConfig: {
                        buttonId: "ok"
                        , seconds: 10
                    }
                })
            } else this.messageBox.showError(i)
    }
    , onUploadHandlerLoad: function (n) {
        this.lastUploadMethod = n.uploadMethod;
        var t = this.uploadMethodMenu.query("[text=" + n.uploadMethod + "]")[0];
        t.setChecked(!0);
        this.multiView.setEmptyText(n.uploadMethodFeatures.dragAndDrop ? this.emptyViewText : "");
        this.bottomToolbar.toggleItemsUICondition(this.uiConditionFn);
        this.fireEventEx("ready", new GleamTech.UI.EventArgs({
            uploadMethod: n.uploadMethod
            , uploadMethodFeatures: n.uploadMethodFeatures
        }))
    }
    , onUploadHandlerLoadError: function (n, t) {
        var r, i, u;
        this.lastUploadMethod && (r = this.uploadMethodMenu.query("[text=" + this.lastUploadMethod + "]")[0], r.setChecked(!0));
        i = Ext.browser.name + " " + Ext.browser.version.major;
        u = Ext.isArray(t) ? GleamTech.Util.Language.getEntry("Message.Error.AnyUploadMethod", t.join(", "), i) : GleamTech.Util.Language.getEntry("Message.Error.SpecificUploadMethod", t, i);
        this.messageBox.showWarning(u)
    }
    , onUploadHandlerUnload: function () {
        if (this.onActionClear(), this.lastUploadMethod) {
            var n = this.uploadMethodMenu.query("[text=" + this.lastUploadMethod + "]")[0];
            n.setChecked(!1)
        }
        this.multiView.setEmptyText("");
        this.bottomToolbar.toggleItemsUICondition(this.uiConditionFn)
    }
    , onUploadHandlerAddFiles: function (n, t) {
        for (var r, f, i, u, h = [], e = [], o = [], s = [], c = 0; c < t.length; c++) {
            if (r = t[c], this.viewStore.getByName(r.data.name)) {
                e.push(r);
                continue
            }
            if (!this.isFileTypeAllowed(r)) {
                o.push(r);
                continue
            }
            if (!this.isFileSizeAllowed(r)) {
                s.push(r);
                continue
            }
            h.push(r);
            r.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Pending))
        } (this.autoStart || this.autoStartOnce) && this.multiView.lockViewLayout("details");
        this.viewStore.add(h);
        f = [];
        e.length > 0 && (i = GleamTech.Util.Language.getEntry("Message.Error.ItemsAlreadyAdded"), f.push({
            message: i
            , records: e
        }));
        o.length > 0 && (i = GleamTech.Util.Language.getEntry("Message.Error.ItemsFileTypeNotAllowed"), i += "\n", this.allowedFileTypes.text && (i += "\n" + GleamTech.Util.Language.getEntry("Label.Info.AllowedFileTypes", "<b>" + this.allowedFileTypes.text + "<\/b>")), this.deniedFileTypes.text && (i += "\n" + GleamTech.Util.Language.getEntry("Label.Info.DeniedFileTypes", "<b>" + this.deniedFileTypes.text + "<\/b>")), f.push({
            message: i
            , records: o
        }));
        s.length > 0 && (i = GleamTech.Util.Language.getEntry("Message.Error.ItemsFileSizeNotAllowed"), i += "\n", i += "\n" + GleamTech.Util.Language.getEntry("Label.Info.AllowedMaxFileSize", "<b>" + GleamTech.Util.Culture.formatKBSize(this.maxFileSizeInBytes) + "<\/b>"), f.push({
            message: i
            , records: s
        }));
        u = this;
        this.showAddConflicts(f, function () {
            Ext.Array.forEach(e, function (t) {
                n.destroyFile(t)
            });
            Ext.Array.forEach(o, function (t) {
                n.destroyFile(t)
            });
            Ext.Array.forEach(s, function (t) {
                n.destroyFile(t)
            });
            (u.autoStart || u.autoStartOnce) && (delete u.autoStartOnce, h.length ? u.start() : u.multiView.unlockViewLayout(!0))
        })
    }
    , isFileTypeAllowed: function (n) {
        var t = n.data.nameWithoutPath;
        return this.deniedFileTypes.regExp && this.deniedFileTypes.regExp.test(t) ? !1 : this.allowedFileTypes.regExp ? this.allowedFileTypes.regExp.test(t) : !0
    }
    , isFileSizeAllowed: function (n) {
        if (!this.maxFileSizeInBytes) return !0;
        var t = n.data.size;
        return t !== undefined && t > this.maxFileSizeInBytes ? !1 : !0
    }
    , onUploadHandlerStartBusy: function () {
        this.busyManager.setBusy(!0)
    }
    , onUploadHandlerEndBusy: function () {
        this.busyManager.setBusy(!1)
    }
    , onUploadProgress: function (n, t) {
        if (n.data.status.value !== GleamTech.FileUltimate.UploadStatus.Canceled) {
            t = t || 0;
            var i = n.data.size > 0 ? t / n.data.size : 0;
            n.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Uploading, null, i));
            this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.FileProcessing, {
                fileProcessedSize: t
                , fileProgress: i
            })
        }
    }
    , onUploadHandlerFileProgress: function (n, t, i) {
        this.onUploadProgress(t, i)
    }
    , onUploadHandlerFileEnd: function (n, t, i) {
        var r, u;
        if (t.data.status.value !== GleamTech.FileUltimate.UploadStatus.Canceled) {
            if (i.errorResult)
                if (i.errorResult.message.indexOf("UploadCanceled") !== -1) t.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Canceled));
                else {
                    i.errorResult.title += " - " + t.data.name;
                    this.onFail(t, i.errorResult)
                }
            else i.successResult && (r = i.successResult, t.data.size === null && t.set("size", r)), t.set("status", this.getStatusData(GleamTech.FileUltimate.UploadStatus.Completed)), this.fireEventEx("itemuploaded", new GleamTech.UI.EventArgs({
                item: this.getEventItems([t])[0]
                , uploadMethod: this.uploadHandler.uploadMethod
                , uploadMethodFeatures: this.uploadHandler.uploadMethodFeatures
            }));
            this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.FileProcessing, {
                fileProcessedSize: t.data.size || 0
                , fileProgress: 1
            });
            this.onQueueStatusChange(GleamTech.FileUltimate.QueueStatus.FileStopped);
            u = this;
            setTimeout(function () {
                u.onUploadNext()
            }, 0)
        }
    }
    , onActionSelectAll: function () {
        this.multiView.getSelectionModel()
            .selectAll()
    }
    , onActionSelectNone: function () {
        this.multiView.getSelectionModel()
            .deselectAll()
    }
    , onActionInvertSelection: function () {
        var n = this.multiView.getSelectionModel();
        n.selectAll(!0);
        n.deselect(this.viewSelection)
    }
    , onActionToggleLayoutExtraLargeIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("extralargeicons")
    }
    , onActionToggleLayoutLargeIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("largeicons")
    }
    , onActionToggleLayoutMediumIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("mediumicons")
    }
    , onActionToggleLayoutSmallIcons: function (n) {
        n.pressed && this.multiView.setViewLayout("smallicons")
    }
    , onActionToggleLayoutList: function (n) {
        n.pressed && this.multiView.setViewLayout("list")
    }
    , onActionToggleLayoutDetails: function (n) {
        n.pressed && this.multiView.setViewLayout("details")
    }
    , onActionToggleLayoutTiles: function (n) {
        n.pressed && this.multiView.setViewLayout("tiles")
    }
    , onActionToggleLayoutContent: function (n) {
        n.pressed && this.multiView.setViewLayout("content")
    }
    , onActionToggleItemCheckBoxes: function (n) {
        this.multiView.setCheckboxSelection(n.checked)
    }
    , onActionAddFiles: function () { }
    , onActionAddFolders: function () { }
    , onActionRemove: function () {
        var n = this.viewSelection;
        this.removeFiles(n)
    }
    , onActionClear: function () {
        var n = this.viewStore.getData()
            .items;
        this.removeFiles(n)
    }
    , removeFiles: function (n) {
        for (var r, i = this.viewStore.remove(n), t = 0; t < i.length; t++) r = i[t], this.uploadHandler.destroyFile(r)
    }
    , onActionUploadMethod: function () {
        var n = this.uploadMethodMenu.query("[checked=true]")[0]
            , t = n.text;
        this.uploadHandler.load(t)
    }
    , onActionViewInfo: function () {
        var t = Ext.browser.name + " " + Ext.browser.version.major
            , n = Ext.String.format("{0} ({1})", this.uploadHandler.uploadMethod, t);
        n += "<br/><br/>";
        Ext.Object.each(this.uploadHandler.uploadMethodFeatures, function (t, i) {
            n += Ext.String.format('{0}: <span style="color:{1}">{2}<\/span><br/>', t, i ? "green" : "red", GleamTech.Util.Language.getEntry(i ? "Label.Yes" : "Label.No"))
        });
        this.messageBox.showInfo(n)
    }
    , onActionUpload: function () {
        this.start()
    }
    , onActionCancel: function () {
        var n = this.viewSelection;
        this.onCancel(n)
    }
    , onActionCancelAll: function () {
        this.stop()
    }
});
Ext.define("GleamTech.FileUltimate.UploadModel", {
    extend: "Ext.data.Model"
    , fields: [{
        name: "itemType"
    }, {
        name: "name"
        , sortType: "asLocaleString"
        , convert: function (n) {
            return GleamTech.Util.String.trim(n.replace(/\//g, "\\"), "\\")
        }
    }, {
        name: "dateModified"
        , type: "date"
        , dateFormat: "c"
    }, {
        name: "size"
    }, {
        name: "status"
        , sortType: function (n) {
            n = n || {};
            var t = n.value || 0;
            return n.progress && (t += n.progress), t
        }
    }, {
        name: "nameWithoutPath"
        , calculate: function (n) {
            var t = n.name;
            return t.substr(t.lastIndexOf("\\") + 1)
        }
    }, {
        name: "extension"
        , calculate: function (n) {
            return n.itemType === GleamTech.FileUltimate.UploadItemType.File ? GleamTech.Util.Path.getExtension(n.name, !0) : ""
        }
    }, {
        name: "type"
        , sortType: "asLocaleString"
        , calculate: function (n) {
            return n.itemType === GleamTech.FileUltimate.UploadItemType.File ? n.extension ? GleamTech.Util.Language.getEntry("Label.FileType", n.extension.toUpperCase()) : GleamTech.Util.Language.getEntry("Label.File") : GleamTech.Util.Language.getEntry("Label.FolderType")
        }
    }, {
        name: "fileRef"
    }, {
        name: "thumbnails"
    }]
});
Ext.define("GleamTech.FileUltimate.UploadStore", {
    extend: "Ext.data.Store"
    , constructor: function () {
        this.callParent(arguments);
        this.nameIndexer = {};
        this.pendingIndexer = {};
        this.on({
            scope: this
            , add: this.onIndexerAdd
            , remove: this.onIndexerRemove
            , update: this.onIndexerUpdate
        })
    }
    , onIndexerAdd: function (n, t) {
        for (var i, u, r = 0; r < t.length; r++) i = t[r], u = this.getIndexerKey(i.data.name), this.nameIndexer[u] = i, i.data.status && i.data.status.value === GleamTech.FileUltimate.UploadStatus.Pending && (this.pendingIndexer[i.data.id] = i)
    }
    , onIndexerRemove: function (n, t) {
        for (var r, u, i = 0; i < t.length; i++) r = t[i], u = this.getIndexerKey(r.data.name), delete this.nameIndexer[u], delete this.pendingIndexer[r.data.id]
    }
    , onIndexerUpdate: function (n, t, i, r) {
        var u, f, e;
        if (i === "edit")
            for (u = 0; u < r.length; u++) switch (r[u]) {
                case "name":
                    f = this.getIndexerKey(t.modified.name);
                    delete this.nameIndexer[f];
                    e = this.getIndexerKey(t.data.name);
                    this.nameIndexer[e] = t;
                    break;
                case "status":
                    t.data.status && t.data.status.value === GleamTech.FileUltimate.UploadStatus.Pending ? this.pendingIndexer[t.data.id] = t : delete this.pendingIndexer[t.data.id]
            }
    }
    , getIndexerKey: function (n) {
        return n.toLowerCase()
    }
    , getByName: function (n) {
        var t = this.getIndexerKey(n);
        return this.nameIndexer[t]
    }
    , hasAnyPending: function () {
        return !Ext.Object.isEmpty(this.pendingIndexer)
    }
});
Ext.define("GleamTech.FileUltimate.QueueStatusViewModel", {
    extend: "Ext.app.ViewModel"
    , alias: "viewmodel.queuestatus"
    , data: {
        inProgress: !1
        , uploadId: ""
        , totalCount: 0
        , totalSize: null
        , startTime: 0
        , processedSize: 0
        , processedCount: 0
        , progress: 0
        , fileProcessedSize: 0
        , fileProgress: 0
    }
    , formulas: {
        currentProcessedSize: function (n) {
            return n("processedSize") + n("fileProcessedSize")
        }
        , timeRemaining: function (n) {
            if (n("totalSize") === null) return "-";
            if (n("currentProcessedSize") === 0) return GleamTech.Util.Language.getEntry("Label.Calculating");
            var e = ((new Date)
                .getTime() - n("startTime")) / 1e3
                , o = n("totalSize") - n("currentProcessedSize")
                , r = Math.round(o * e / n("currentProcessedSize"))
                , f = parseInt(r / 3600)
                , i = parseInt(r / 60) % 60
                , u = r % 60
                , t;
            return f > 0 ? (t = GleamTech.Util.Language.getEntry("Label.TimeEstimateHours", f), i > 0 && (t += " " + GleamTech.Util.Language.getEntry("Label.TimeEstimateMinutes", i))) : i > 0 ? (t = GleamTech.Util.Language.getEntry("Label.TimeEstimateMinutes", i), u > 0 && (t += " " + GleamTech.Util.Language.getEntry("Label.TimeEstimateSeconds", u))) : t = GleamTech.Util.Language.getEntry("Label.TimeEstimateSeconds", u), GleamTech.Util.Language.getEntry("Label.TimeEstimate", t)
        }
        , itemsRemaining: function (n) {
            var t = n("totalCount") - n("processedCount")
                , i;
            return n("totalSize") === null ? t : (i = n("totalSize") - n("currentProcessedSize"), t + " (" + GleamTech.Util.Culture.formatByteSize(i) + ")")
        }
        , speed: function (n) {
            var t, i;
            return n("currentProcessedSize") === 0 ? GleamTech.Util.Language.getEntry("Label.Calculating") : (t = ((new Date)
                .getTime() - n("startTime")) / 1e3, t = (t - t % .1)
                    .toFixed(1), i = t > 0 ? n("currentProcessedSize") / t : 0, GleamTech.Util.Language.getEntry("Label.SpeedEstimate", GleamTech.Util.Culture.formatByteSize(i)))
        }
        , progress: function (n) {
            return n("totalSize") !== null ? n("totalSize") === 0 ? 0 : n("currentProcessedSize") / n("totalSize") : n("totalCount") === 0 ? 0 : n("totalCount") === n("processedCount") ? 1 : (n("processedCount") + n("fileProgress")) / n("totalCount")
        }
    }
});
GleamTech.FileUltimate.UploadItemType = {
    Folder: 1
    , File: 2
};
GleamTech.FileUltimate.UploadStatus = {
    Pending: 1
    , Rejected: 2
    , Skipped: 3
    , Uploading: 4
    , Canceled: 5
    , Failed: 6
    , Completed: 7
};
GleamTech.FileUltimate.QueueStatus = {
    Starting: 1
    , Started: 2
    , FileStarted: 3
    , FileProcessing: 4
    , FileStopped: 5
    , Stopped: 6
};
GleamTech.FileUltimate.ValidationAction = {
    Accept: 0
    , Reject: 1
    , ConfirmReplace: 2
};
Ext.define("GleamTech.FileUltimate.UploadHandler", {
    mixins: ["Ext.mixin.Observable"]
    , statics: {
        supportedUploadMethods: []
        , supportedThumbnailExtensions: []
    }
    , $configPrefixed: !1
    , config: {
        resourceBasePath: ""
        , shimContainer: null
        , selectFileButton: null
        , selectFolderButton: null
        , dropTarget: null
        , uploadMethods: ""
        , chunkSizeInBytes: 0
        , mimeTypes: []
    }
    , uploadMethod: null
    , uploadMethodFeatures: {
        selectFile: !1
        , selectMultiple: !1
        , selectFolder: !1
        , dragAndDrop: !1
        , displayThumbnails: !1
        , sendAsChunks: !1
        , sendAsMultipart: !1
        , reportProgress: !1
        , reportFileSize: !1
        , reportFileDate: !1
    }
    , thumbnailExtensionsMap: null
    , constructor: function (n) {
        this.initConfig(n);
        this.mixins.observable.constructor.call(this, n);
        this.tryLoad()
    }
    , destroy: function () {
        this.destroying = !0;
        this.unload();
        this.destroyed = !0;
        delete this.destroying
    }
    , applyUploadMethods: function (n) {
        for (var f, i, r = [], u = n.split(/\s?,\s?/), e = this.self.supportedUploadMethods, t = 0; t < u.length; t++) f = u[t], i = Ext.Array.findBy(e, function (n) {
            return f.toLowerCase() === n.toLowerCase()
        }), i && r.push(i);
        return r
    }
    , tryLoad: function () {
        this.tryIndex === undefined && (this.tryIndex = 0);
        this.load(this.uploadMethods[this.tryIndex])
    }
    , load: function () { }
    , unload: function () { }
    , destroyFile: function () { }
    , addFiles: function () { }
    , isThumbnailExtension: function (n) {
        return this.thumbnailExtensionsMap || (this.thumbnailExtensionsMap = Ext.Array.toMap(this.self.supportedThumbnailExtensions)), this.thumbnailExtensionsMap[n.toLowerCase()]
    }
    , getThumbnail: function () { }
    , uploadFile: function () { }
    , cancelFile: function () { }
    , stop: function () { }
    , onBeforeLoad: function (n) {
        return this.uploadMethod === n ? !1 : (this.onStartBusy(), this.loadingUploadMethod = n, !0)
    }
    , onAfterLoad: function (n) {
        this.onEndBusy();
        delete this.tryIndex;
        this.uploadMethod = this.loadingUploadMethod;
        delete this.loadingUploadMethod;
        this.uploadMethodFeatures = n;
        this.fireEvent("load", this)
    }
    , onLoadError: function () {
        this.onEndBusy();
        var n = null;
        if (this.tryIndex !== undefined) {
            if (this.tryIndex++ , this.tryIndex < this.uploadMethods.length) {
                this.tryLoad();
                return
            }
            delete this.tryIndex;
            n = this.uploadMethods
        }
        n || (n = this.loadingUploadMethod);
        delete this.loadingUploadMethod;
        this.fireEvent("loaderror", this, n)
    }
    , onAfterUnload: function () {
        var n = this.uploadMethod !== null;
        this.uploadMethod = null;
        this.uploadMethodFeatures = null;
        n && !this.destroying && this.fireEvent("unload", this)
    }
    , onAddFiles: function (n) {
        this.fireEvent("addfiles", this, n)
    }
    , onStartBusy: function () {
        this.fireEvent("startbusy", this)
    }
    , onEndBusy: function () {
        this.fireEvent("endbusy", this)
    }
    , onFileProgress: function (n, t) {
        this.fireEvent("fileprogress", this, n, t)
    }
    , onHttpResponse: function (n) {
        return GleamTech.UI.ServerHandler.parseResponse(n)
    }
    , onFileEnd: function (n, t) {
        this.fireEvent("fileend", this, n, t)
    }
});
Ext.define("GleamTech.FileUltimate.PlUploadHandler", {
    extend: "GleamTech.FileUltimate.UploadHandler"
    , statics: {
        supportedUploadMethods: ["Html5", "Silverlight", "Flash", "Html4"]
        , supportedThumbnailExtensions: ["jpg", "jpeg", "jpe", "jif", "jfif", "jfi", "exif", "png"]
    }
    , mimeTypes: []
    , constructor: function () {
        this.callParent(arguments)
    }
    , destroy: function () {
        this.callParent(arguments)
    }
    , applyMimeTypes: function (n) {
        for (var i, r, u = n, f = !0, e = [], o = [], t = 0; t < u.length; t++) {
            if (i = u[t], r = i.match(/^\*\.([^\*\?]+)$/), r === null) {
                f = !1;
                break
            }
            o.push(i);
            e.push(r[1])
        }
        return f ? [{
            title: GleamTech.Util.Language.getEntry("Label.SpecificFiles", o.join(";"))
            , extensions: e.join(",")
        }] : []
    }
    , updateMimeTypes: function (n) {
        this.uploader && this.uploader.setOption({
            filters: Ext.apply(this.uploader.getOption("filters"), {
                mime_types: n
            })
        })
    }
    , updateChunkSizeInBytes: function (n) {
        this.uploader && this.uploader.setOption({
            chunk_size: n
        })
    }
    , load: function (n) {
        var t, i;
        if (this.onBeforeLoad(n)) {
            t = {
                container: this.shimContainer
                , browse_button: this.selectFileButton
                , drop_element: this.dropTarget
                , filters: {
                    mime_types: this.mimeTypes
                    , skipMimeTypesCheck: !0
                    , prevent_empty: !1
                }
                , chunk_size: this.chunkSizeInBytes
                , flash_swf_url: GleamTech.Util.Path.combine(this.resourceBasePath, "library/plupload/Moxie.swf")
                , silverlight_xap_url: GleamTech.Util.Path.combine(this.resourceBasePath, "library/plupload/Moxie.xap")
            };
            Ext.Object.each(t, function (n, t, i) {
                t || delete i[n]
            });
            switch (n) {
                case "Html4":
                    Ext.apply(t, {
                        runtimes: "html4"
                    });
                    break;
                case "Flash":
                    Ext.apply(t, {
                        runtimes: "flash"
                        , multipart: !1
                    });
                    break;
                case "Silverlight":
                    Ext.apply(t, {
                        runtimes: "silverlight"
                        , multipart: !1
                    });
                    break;
                case "Html5":
                    Ext.apply(t, {
                        runtimes: "html5"
                        , multipart: !1
                        , required_features: "chunks"
                    });
                    break;
                default:
                    this.onLoadError();
                    return
            }
            i = new plupload.Uploader(t);
            Ext.each(["Init", "Error", "FilesAdded", "UploadProgress", "ChunkUploaded", "FileUploaded", "StartResolveFiles", "EndResolveFiles"], function (n) {
                i.bind(n, this["onPlUpload" + n], this)
            }, this);
            i.init()
        }
    }
    , unload: function () {
        this.folderInput && (this.folderInput.destroy(), this.folderInput = null);
        this.uploader && (this.uploader.destroy(), this.uploader = null);
        this.onAfterUnload()
    }
    , destroyFile: function (n) {
        var t = n.data.fileRef;
        t.destroy();
        n.destroy()
    }
    , addFiles: function (n) {
        this.uploader && this.uploader.addFile(n)
    }
    , getThumbnail: function (n, t, i, r) {
        var o = n.data.fileRef
            , f = function () {
                this.destroy();
                this.parentImage && this.parentImage.destroy();
                r()
            }
            , e = function () {
                var n, r;
                if (!this.parentImage && this.meta.thumb && (this.meta.thumb.width >= t || this.meta.thumb.height >= t)) {
                    n = new moxie.image.Image;
                    n.parentImage = this;
                    n.bind("error", f);
                    n.bind("load", e);
                    n.load(this.meta.thumb.data);
                    return
                }
                this.resize({
                    width: t
                    , height: t
                    , preserveHeaders: !1
                    , fit: !1
                });
                r = this.getAsDataURL(this.type);
                this.destroy();
                this.parentImage && this.parentImage.destroy();
                i(r)
            }
            , u = new moxie.image.Image;
        u.bind("error", f);
        u.bind("load", e);
        u.load(o.getSource())
    }
    , uploadFile: function (n, t) {
        this.uploader.settings.url = t;
        this.uploader.settings.send_file_name = !1;
        this.uploader.state !== plupload.STARTED && (this.uploader.state = plupload.STARTED, this.uploader.trigger("StateChanged"));
        var i = n.data.fileRef;
        i.status = plupload.UPLOADING;
        this.uploader.trigger("UploadFile", i)
    }
    , cancelFile: function (n) {
        var t = n.data.fileRef
            , i = t.status === plupload.UPLOADING;
        t.status = plupload.FAILED;
        i && this.uploader.trigger("CancelUpload")
    }
    , stop: function () {
        this.uploader.stop()
    }
    , onPlUploadInit: function (n, t) {
        if (n !== this.uploader) {
            this.unload();
            this.uploader = n;
            var i = {
                selectFile: t.runtime.can("select_file")
                , selectMultiple: t.runtime.can("select_multiple")
                , selectFolder: t.runtime.can("select_folder")
                , triggerDialog: t.runtime.can("summon_file_dialog")
                , dragAndDrop: t.runtime.can("drag_and_drop")
                , displayThumbnails: t.runtime.can("access_image_binary") && t.runtime.can("resize_image") && t.runtime.can("display_media")
                , sendAsChunks: t.runtime.can("send_binary_string") && t.runtime.can("slice_blob")
                , sendAsMultipart: t.runtime.can("send_multipart")
                , reportProgress: t.runtime.can("report_upload_progress") && t.runtime.type !== "flash" && t.runtime.type !== "silverlight"
                , reportFileSize: t.runtime.type !== "html4"
                , reportFileDate: t.runtime.type !== "html4" && t.runtime.type !== "silverlight"
            };
            i.selectFolder && this.initFolderInput();
            this.onAfterLoad(i)
        }
    }
    , onPlUploadError: function (n, t) {
        if (t.code === plupload.INIT_ERROR) n.destroy(), this.onLoadError();
        else if (t.code === plupload.HTTP_ERROR) {
            var i = this.onHttpResponse(this.getResponse(t.xhr));
            t.file.status = plupload.FAILED;
            this.onFileEnd(t.file.recordRef, i)
        }
        return !1
    }
    , onPlUploadFilesAdded: function (n, t) {
        for (var i, u, f = [], r = 0; r < t.length; r++) i = t[r], u = new GleamTech.FileUltimate.UploadModel({
            id: i.id
            , itemType: GleamTech.FileUltimate.UploadItemType.File
            , name: i.relativePath || i.name
            , dateModified: this.uploadMethodFeatures.reportFileDate ? i.lastModifiedDate : null
            , size: this.uploadMethodFeatures.reportFileSize ? i.size : null
            , fileRef: i
        }), f.push(u), i.recordRef = u;
        this.onAddFiles(f);
        return !1
    }
    , onPlUploadUploadProgress: function (n, t) {
        if (this.uploadMethodFeatures.reportProgress) this.onFileProgress(t.recordRef, t.loaded);
        return !1
    }
    , onPlUploadChunkUploaded: function (n, t, i) {
        var r = this.onHttpResponse(this.getResponse(i.xhr));
        if (r.errorResult) {
            t.status = plupload.FAILED;
            this.onFileEnd(t.recordRef, r)
        }
        return !1
    }
    , onPlUploadFileUploaded: function (n, t, i) {
        var r = this.onHttpResponse(this.getResponse(i.xhr));
        r.errorResult && (t.status = plupload.FAILED);
        this.onFileEnd(t.recordRef, r);
        return !1
    }
    , onPlUploadStartResolveFiles: function () {
        this.onStartBusy()
    }
    , onPlUploadEndResolveFiles: function () {
        this.onEndBusy()
    }
    , initFolderInput: function () {
        this.folderInput = new moxie.file.FileInput({
            browse_button: this.selectFolderButton
            , directory: !0
        });
        var n = this;
        this.folderInput.bind("change", function () {
            n.uploader.addFile(n.folderInput.files)
        });
        this.folderInput.init()
    }
    , getResponse: function (n) {
        var t = Ext.data.request.Ajax.parseStatus(n.status, n);
        return {
            success: t.success
            , status: t.isException ? 0 : n.status
            , statusText: n.statusText
            , contentType: "application/json"
            , responseText: n.responseText
        }
    }
});
Ext.define("GleamTech.FileUltimate.FileDropHelper", {
    singleton: !0
    , getFilesFromEvent: function (n, t, i) {
        this.fileDrop || (this.fileDrop = new Html5FileDrop);
        this.fileDrop.getFromEvent(n, function (n) {
            if (n.length) t(n);
            else {
                var r = Ext.browser.name + " " + Ext.browser.version.major
                    , u = GleamTech.Util.Language.getEntry("Message.DropNotRecognized", r);
                i(u)
            }
        })
    }
});
Ext.define("GleamTech.UI.ProgressWithStatus", {
    extend: "Ext.Progress"
    , xtype: ["progresswithstatus", "progresswithstatuswidget"]
    , updateValue: function (n) {
        if (this.statusData = Ext.isObject(n) ? n : null, this.statusData && this.statusData.text) {
            if (this.element.addCls("x-progress-with-status"), this.backgroundEl.setStyle({
                color: this.statusData.color
                , textDecoration: this.statusData.details ? "underline" : ""
                , cursor: this.statusData.details ? "pointer" : ""
            }), this.statusData.details) {
                this.backgroundEl.on("click", this.onStatusClick, this);
                this.backgroundEl.dom.title = GleamTech.Util.Language.getEntry("Label.ViewStatusReason")
            } else this.backgroundEl.un("click", this.onStatusClick, this), this.backgroundEl.dom.title = "";
            this.hasStatusText = !0;
            this.setText(this.statusData.text)
        } else this.hasStatusText && (this.element.removeCls("x-progress-with-status"), this.backgroundEl.setStyle({
            color: ""
            , textDecoration: ""
            , cursor: ""
        }), this.backgroundEl.un("click", this.onStatusClick, this), this.backgroundEl.dom.title = "", delete this.hasStatusText), this.callParent([this.statusData ? this.statusData.progress : n])
    }
    , onStatusClick: function () {
        Ext.callback(this.showDetails, this, [this.statusData])
    }
    , getTdType: function () {
        return this.self.superclass.xtype
    }
});

GleamTech.FileUltimate.FileTypeInfo.baseFileIcons = {
    "file": {
        "16": "basefileicons16 icon-file"
        , "32": "basefileicons32 icon-file"
        , "48": "basefileicons48 icon-file"
        , "96": "basefileicons96-icon-file"
        , "256": "basefileicons256-icon-file"
    }
    , "folder": {
        "16": "basefileicons16 icon-folder"
        , "32": "basefileicons32 icon-folder"
        , "48": "basefileicons48 icon-folder"
        , "96": "basefileicons96-icon-folder"
        , "256": "basefileicons256-icon-folder"
    }
    , "rootfolder": {
        "16": "basefileicons16 icon-rootfolder"
        , "32": "basefileicons32 icon-rootfolder"
        , "48": "basefileicons48 icon-rootfolder"
        , "96": "basefileicons96-icon-rootfolder"
        , "256": "basefileicons256-icon-rootfolder"
    }
};
GleamTech.FileUltimate.FileTypeInfo.fileIcons = {
    "7z": {
        "16": "fileicons-archive16 icon-7z"
        , "32": "fileicons-archive32 icon-7z"
        , "48": "fileicons-archive48 icon-7z"
        , "96": "fileicons-archive96-icon-7z"
        , "256": "fileicons-archive256-icon-7z"
    }
    , "bz2": {
        "16": "fileicons-archive16 icon-bz2"
        , "32": "fileicons-archive32 icon-bz2"
        , "48": "fileicons-archive48 icon-bz2"
        , "96": "fileicons-archive96-icon-bz2"
        , "256": "fileicons-archive256-icon-bz2"
    }
    , "dmg": {
        "16": "fileicons-archive16 icon-dmg"
        , "32": "fileicons-archive32 icon-dmg"
        , "48": "fileicons-archive48 icon-dmg"
        , "96": "fileicons-archive96-icon-dmg"
        , "256": "fileicons-archive256-icon-dmg"
    }
    , "gz": {
        "16": "fileicons-archive16 icon-gz"
        , "32": "fileicons-archive32 icon-gz"
        , "48": "fileicons-archive48 icon-gz"
        , "96": "fileicons-archive96-icon-gz"
        , "256": "fileicons-archive256-icon-gz"
    }
    , "iso": {
        "16": "fileicons-archive16 icon-iso"
        , "32": "fileicons-archive32 icon-iso"
        , "48": "fileicons-archive48 icon-iso"
        , "96": "fileicons-archive96-icon-iso"
        , "256": "fileicons-archive256-icon-iso"
    }
    , "jar": {
        "16": "fileicons-archive16 icon-jar"
        , "32": "fileicons-archive32 icon-jar"
        , "48": "fileicons-archive48 icon-jar"
        , "96": "fileicons-archive96-icon-jar"
        , "256": "fileicons-archive256-icon-jar"
    }
    , "pkg": {
        "16": "fileicons-archive16 icon-pkg"
        , "32": "fileicons-archive32 icon-pkg"
        , "48": "fileicons-archive48 icon-pkg"
        , "96": "fileicons-archive96-icon-pkg"
        , "256": "fileicons-archive256-icon-pkg"
    }
    , "rar": {
        "16": "fileicons-archive16 icon-rar"
        , "32": "fileicons-archive32 icon-rar"
        , "48": "fileicons-archive48 icon-rar"
        , "96": "fileicons-archive96-icon-rar"
        , "256": "fileicons-archive256-icon-rar"
    }
    , "sitx": {
        "16": "fileicons-archive16 icon-sitx"
        , "32": "fileicons-archive32 icon-sitx"
        , "48": "fileicons-archive48 icon-sitx"
        , "96": "fileicons-archive96-icon-sitx"
        , "256": "fileicons-archive256-icon-sitx"
    }
    , "tar": {
        "16": "fileicons-archive16 icon-tar"
        , "32": "fileicons-archive32 icon-tar"
        , "48": "fileicons-archive48 icon-tar"
        , "96": "fileicons-archive96-icon-tar"
        , "256": "fileicons-archive256-icon-tar"
    }
    , "xz": {
        "16": "fileicons-archive16 icon-xz"
        , "32": "fileicons-archive32 icon-xz"
        , "48": "fileicons-archive48 icon-xz"
        , "96": "fileicons-archive96-icon-xz"
        , "256": "fileicons-archive256-icon-xz"
    }
    , "zip": {
        "16": "fileicons-archive16 icon-zip"
        , "32": "fileicons-archive32 icon-zip"
        , "48": "fileicons-archive48 icon-zip"
        , "96": "fileicons-archive96-icon-zip"
        , "256": "fileicons-archive256-icon-zip"
    }
    , "aac": {
        "16": "fileicons-audio16 icon-aac"
        , "32": "fileicons-audio32 icon-aac"
        , "48": "fileicons-audio48 icon-aac"
        , "96": "fileicons-audio96-icon-aac"
        , "256": "fileicons-audio256-icon-aac"
    }
    , "m4a": {
        "16": "fileicons-audio16 icon-m4a"
        , "32": "fileicons-audio32 icon-m4a"
        , "48": "fileicons-audio48 icon-m4a"
        , "96": "fileicons-audio96-icon-m4a"
        , "256": "fileicons-audio256-icon-m4a"
    }
    , "midi": {
        "16": "fileicons-audio16 icon-midi"
        , "32": "fileicons-audio32 icon-midi"
        , "48": "fileicons-audio48 icon-midi"
        , "96": "fileicons-audio96-icon-midi"
        , "256": "fileicons-audio256-icon-midi"
    }
    , "mp3": {
        "16": "fileicons-audio16 icon-mp3"
        , "32": "fileicons-audio32 icon-mp3"
        , "48": "fileicons-audio48 icon-mp3"
        , "96": "fileicons-audio96-icon-mp3"
        , "256": "fileicons-audio256-icon-mp3"
    }
    , "wav": {
        "16": "fileicons-audio16 icon-wav"
        , "32": "fileicons-audio32 icon-wav"
        , "48": "fileicons-audio48 icon-wav"
        , "96": "fileicons-audio96-icon-wav"
        , "256": "fileicons-audio256-icon-wav"
    }
    , "wma": {
        "16": "fileicons-audio16 icon-wma"
        , "32": "fileicons-audio32 icon-wma"
        , "48": "fileicons-audio48 icon-wma"
        , "96": "fileicons-audio96-icon-wma"
        , "256": "fileicons-audio256-icon-wma"
    }
    , "3ds": {
        "16": "fileicons-creative16 icon-3ds"
        , "32": "fileicons-creative32 icon-3ds"
        , "48": "fileicons-creative48 icon-3ds"
        , "96": "fileicons-creative96-icon-3ds"
        , "256": "fileicons-creative256-icon-3ds"
    }
    , "ai": {
        "16": "fileicons-creative16 icon-ai"
        , "32": "fileicons-creative32 icon-ai"
        , "48": "fileicons-creative48 icon-ai"
        , "96": "fileicons-creative96-icon-ai"
        , "256": "fileicons-creative256-icon-ai"
    }
    , "as": {
        "16": "fileicons-creative16 icon-as"
        , "32": "fileicons-creative32 icon-as"
        , "48": "fileicons-creative48 icon-as"
        , "96": "fileicons-creative96-icon-as"
        , "256": "fileicons-creative256-icon-as"
    }
    , "cdr": {
        "16": "fileicons-creative16 icon-cdr"
        , "32": "fileicons-creative32 icon-cdr"
        , "48": "fileicons-creative48 icon-cdr"
        , "96": "fileicons-creative96-icon-cdr"
        , "256": "fileicons-creative256-icon-cdr"
    }
    , "dwg": {
        "16": "fileicons-creative16 icon-dwg"
        , "32": "fileicons-creative32 icon-dwg"
        , "48": "fileicons-creative48 icon-dwg"
        , "96": "fileicons-creative96-icon-dwg"
        , "256": "fileicons-creative256-icon-dwg"
    }
    , "dxf": {
        "16": "fileicons-creative16 icon-dxf"
        , "32": "fileicons-creative32 icon-dxf"
        , "48": "fileicons-creative48 icon-dxf"
        , "96": "fileicons-creative96-icon-dxf"
        , "256": "fileicons-creative256-icon-dxf"
    }
    , "eps": {
        "16": "fileicons-creative16 icon-eps"
        , "32": "fileicons-creative32 icon-eps"
        , "48": "fileicons-creative48 icon-eps"
        , "96": "fileicons-creative96-icon-eps"
        , "256": "fileicons-creative256-icon-eps"
    }
    , "fla": {
        "16": "fileicons-creative16 icon-fla"
        , "32": "fileicons-creative32 icon-fla"
        , "48": "fileicons-creative48 icon-fla"
        , "96": "fileicons-creative96-icon-fla"
        , "256": "fileicons-creative256-icon-fla"
    }
    , "indd": {
        "16": "fileicons-creative16 icon-indd"
        , "32": "fileicons-creative32 icon-indd"
        , "48": "fileicons-creative48 icon-indd"
        , "96": "fileicons-creative96-icon-indd"
        , "256": "fileicons-creative256-icon-indd"
    }
    , "prproj": {
        "16": "fileicons-creative16 icon-prproj"
        , "32": "fileicons-creative32 icon-prproj"
        , "48": "fileicons-creative48 icon-prproj"
        , "96": "fileicons-creative96-icon-prproj"
        , "256": "fileicons-creative256-icon-prproj"
    }
    , "psb": {
        "16": "fileicons-creative16 icon-psb"
        , "32": "fileicons-creative32 icon-psb"
        , "48": "fileicons-creative48 icon-psb"
        , "96": "fileicons-creative96-icon-psb"
        , "256": "fileicons-creative256-icon-psb"
    }
    , "psd": {
        "16": "fileicons-creative16 icon-psd"
        , "32": "fileicons-creative32 icon-psd"
        , "48": "fileicons-creative48 icon-psd"
        , "96": "fileicons-creative96-icon-psd"
        , "256": "fileicons-creative256-icon-psd"
    }
    , "pspimage": {
        "16": "fileicons-creative16 icon-pspimage"
        , "32": "fileicons-creative32 icon-pspimage"
        , "48": "fileicons-creative48 icon-pspimage"
        , "96": "fileicons-creative96-icon-pspimage"
        , "256": "fileicons-creative256-icon-pspimage"
    }
    , "svg": {
        "16": "fileicons-creative16 icon-svg"
        , "32": "fileicons-creative32 icon-svg"
        , "48": "fileicons-creative48 icon-svg"
        , "96": "fileicons-creative96-icon-svg"
        , "256": "fileicons-creative256-icon-svg"
    }
    , "c": {
        "16": "fileicons-developer16 icon-c"
        , "32": "fileicons-developer32 icon-c"
        , "48": "fileicons-developer48 icon-c"
        , "96": "fileicons-developer96-icon-c"
        , "256": "fileicons-developer256-icon-c"
    }
    , "cpp": {
        "16": "fileicons-developer16 icon-cpp"
        , "32": "fileicons-developer32 icon-cpp"
        , "48": "fileicons-developer48 icon-cpp"
        , "96": "fileicons-developer96-icon-cpp"
        , "256": "fileicons-developer256-icon-cpp"
    }
    , "cs": {
        "16": "fileicons-developer16 icon-cs"
        , "32": "fileicons-developer32 icon-cs"
        , "48": "fileicons-developer48 icon-cs"
        , "96": "fileicons-developer96-icon-cs"
        , "256": "fileicons-developer256-icon-cs"
    }
    , "csproj": {
        "16": "fileicons-developer16 icon-csproj"
        , "32": "fileicons-developer32 icon-csproj"
        , "48": "fileicons-developer48 icon-csproj"
        , "96": "fileicons-developer96-icon-csproj"
        , "256": "fileicons-developer256-icon-csproj"
    }
    , "dtd": {
        "16": "fileicons-developer16 icon-dtd"
        , "32": "fileicons-developer32 icon-dtd"
        , "48": "fileicons-developer48 icon-dtd"
        , "96": "fileicons-developer96-icon-dtd"
        , "256": "fileicons-developer256-icon-dtd"
    }
    , "h": {
        "16": "fileicons-developer16 icon-h"
        , "32": "fileicons-developer32 icon-h"
        , "48": "fileicons-developer48 icon-h"
        , "96": "fileicons-developer96-icon-h"
        , "256": "fileicons-developer256-icon-h"
    }
    , "ldf": {
        "16": "fileicons-developer16 icon-ldf"
        , "32": "fileicons-developer32 icon-ldf"
        , "48": "fileicons-developer48 icon-ldf"
        , "96": "fileicons-developer96-icon-ldf"
        , "256": "fileicons-developer256-icon-ldf"
    }
    , "mdf": {
        "16": "fileicons-developer16 icon-mdf"
        , "32": "fileicons-developer32 icon-mdf"
        , "48": "fileicons-developer48 icon-mdf"
        , "96": "fileicons-developer96-icon-mdf"
        , "256": "fileicons-developer256-icon-mdf"
    }
    , "pdb": {
        "16": "fileicons-developer16 icon-pdb"
        , "32": "fileicons-developer32 icon-pdb"
        , "48": "fileicons-developer48 icon-pdb"
        , "96": "fileicons-developer96-icon-pdb"
        , "256": "fileicons-developer256-icon-pdb"
    }
    , "resx": {
        "16": "fileicons-developer16 icon-resx"
        , "32": "fileicons-developer32 icon-resx"
        , "48": "fileicons-developer48 icon-resx"
        , "96": "fileicons-developer96-icon-resx"
        , "256": "fileicons-developer256-icon-resx"
    }
    , "sln": {
        "16": "fileicons-developer16 icon-sln"
        , "32": "fileicons-developer32 icon-sln"
        , "48": "fileicons-developer48 icon-sln"
        , "96": "fileicons-developer96-icon-sln"
        , "256": "fileicons-developer256-icon-sln"
    }
    , "sql": {
        "16": "fileicons-developer16 icon-sql"
        , "32": "fileicons-developer32 icon-sql"
        , "48": "fileicons-developer48 icon-sql"
        , "96": "fileicons-developer96-icon-sql"
        , "256": "fileicons-developer256-icon-sql"
    }
    , "suo": {
        "16": "fileicons-developer16 icon-suo"
        , "32": "fileicons-developer32 icon-suo"
        , "48": "fileicons-developer48 icon-suo"
        , "96": "fileicons-developer96-icon-suo"
        , "256": "fileicons-developer256-icon-suo"
    }
    , "vb": {
        "16": "fileicons-developer16 icon-vb"
        , "32": "fileicons-developer32 icon-vb"
        , "48": "fileicons-developer48 icon-vb"
        , "96": "fileicons-developer96-icon-vb"
        , "256": "fileicons-developer256-icon-vb"
    }
    , "vbproj": {
        "16": "fileicons-developer16 icon-vbproj"
        , "32": "fileicons-developer32 icon-vbproj"
        , "48": "fileicons-developer48 icon-vbproj"
        , "96": "fileicons-developer96-icon-vbproj"
        , "256": "fileicons-developer256-icon-vbproj"
    }
    , "vcproj": {
        "16": "fileicons-developer16 icon-vcproj"
        , "32": "fileicons-developer32 icon-vcproj"
        , "48": "fileicons-developer48 icon-vcproj"
        , "96": "fileicons-developer96-icon-vcproj"
        , "256": "fileicons-developer256-icon-vcproj"
    }
    , "vcxproj": {
        "16": "fileicons-developer16 icon-vcxproj"
        , "32": "fileicons-developer32 icon-vcxproj"
        , "48": "fileicons-developer48 icon-vcxproj"
        , "96": "fileicons-developer96-icon-vcxproj"
        , "256": "fileicons-developer256-icon-vcxproj"
    }
    , "xaml": {
        "16": "fileicons-developer16 icon-xaml"
        , "32": "fileicons-developer32 icon-xaml"
        , "48": "fileicons-developer48 icon-xaml"
        , "96": "fileicons-developer96-icon-xaml"
        , "256": "fileicons-developer256-icon-xaml"
    }
    , "xml": {
        "16": "fileicons-developer16 icon-xml"
        , "32": "fileicons-developer32 icon-xml"
        , "48": "fileicons-developer48 icon-xml"
        , "96": "fileicons-developer96-icon-xml"
        , "256": "fileicons-developer256-icon-xml"
    }
    , "xsd": {
        "16": "fileicons-developer16 icon-xsd"
        , "32": "fileicons-developer32 icon-xsd"
        , "48": "fileicons-developer48 icon-xsd"
        , "96": "fileicons-developer96-icon-xsd"
        , "256": "fileicons-developer256-icon-xsd"
    }
    , "xsl": {
        "16": "fileicons-developer16 icon-xsl"
        , "32": "fileicons-developer32 icon-xsl"
        , "48": "fileicons-developer48 icon-xsl"
        , "96": "fileicons-developer96-icon-xsl"
        , "256": "fileicons-developer256-icon-xsl"
    }
    , "bmp": {
        "16": "fileicons-image16 icon-bmp"
        , "32": "fileicons-image32 icon-bmp"
        , "48": "fileicons-image48 icon-bmp"
        , "96": "fileicons-image96-icon-bmp"
        , "256": "fileicons-image256-icon-bmp"
    }
    , "gif": {
        "16": "fileicons-image16 icon-gif"
        , "32": "fileicons-image32 icon-gif"
        , "48": "fileicons-image48 icon-gif"
        , "96": "fileicons-image96-icon-gif"
        , "256": "fileicons-image256-icon-gif"
    }
    , "ico": {
        "16": "fileicons-image16 icon-ico"
        , "32": "fileicons-image32 icon-ico"
        , "48": "fileicons-image48 icon-ico"
        , "96": "fileicons-image96-icon-ico"
        , "256": "fileicons-image256-icon-ico"
    }
    , "jpg": {
        "16": "fileicons-image16 icon-jpg"
        , "32": "fileicons-image32 icon-jpg"
        , "48": "fileicons-image48 icon-jpg"
        , "96": "fileicons-image96-icon-jpg"
        , "256": "fileicons-image256-icon-jpg"
    }
    , "png": {
        "16": "fileicons-image16 icon-png"
        , "32": "fileicons-image32 icon-png"
        , "48": "fileicons-image48 icon-png"
        , "96": "fileicons-image96-icon-png"
        , "256": "fileicons-image256-icon-png"
    }
    , "accdb": {
        "16": "fileicons-office16 icon-accdb"
        , "32": "fileicons-office32 icon-accdb"
        , "48": "fileicons-office48 icon-accdb"
        , "96": "fileicons-office96-icon-accdb"
        , "256": "fileicons-office256-icon-accdb"
    }
    , "csv": {
        "16": "fileicons-office16 icon-csv"
        , "32": "fileicons-office32 icon-csv"
        , "48": "fileicons-office48 icon-csv"
        , "96": "fileicons-office96-icon-csv"
        , "256": "fileicons-office256-icon-csv"
    }
    , "doc": {
        "16": "fileicons-office16 icon-doc"
        , "32": "fileicons-office32 icon-doc"
        , "48": "fileicons-office48 icon-doc"
        , "96": "fileicons-office96-icon-doc"
        , "256": "fileicons-office256-icon-doc"
    }
    , "docm": {
        "16": "fileicons-office16 icon-docm"
        , "32": "fileicons-office32 icon-docm"
        , "48": "fileicons-office48 icon-docm"
        , "96": "fileicons-office96-icon-docm"
        , "256": "fileicons-office256-icon-docm"
    }
    , "docx": {
        "16": "fileicons-office16 icon-docx"
        , "32": "fileicons-office32 icon-docx"
        , "48": "fileicons-office48 icon-docx"
        , "96": "fileicons-office96-icon-docx"
        , "256": "fileicons-office256-icon-docx"
    }
    , "dot": {
        "16": "fileicons-office16 icon-dot"
        , "32": "fileicons-office32 icon-dot"
        , "48": "fileicons-office48 icon-dot"
        , "96": "fileicons-office96-icon-dot"
        , "256": "fileicons-office256-icon-dot"
    }
    , "dotm": {
        "16": "fileicons-office16 icon-dotm"
        , "32": "fileicons-office32 icon-dotm"
        , "48": "fileicons-office48 icon-dotm"
        , "96": "fileicons-office96-icon-dotm"
        , "256": "fileicons-office256-icon-dotm"
    }
    , "dotx": {
        "16": "fileicons-office16 icon-dotx"
        , "32": "fileicons-office32 icon-dotx"
        , "48": "fileicons-office48 icon-dotx"
        , "96": "fileicons-office96-icon-dotx"
        , "256": "fileicons-office256-icon-dotx"
    }
    , "epub": {
        "16": "fileicons-office16 icon-epub"
        , "32": "fileicons-office32 icon-epub"
        , "48": "fileicons-office48 icon-epub"
        , "96": "fileicons-office96-icon-epub"
        , "256": "fileicons-office256-icon-epub"
    }
    , "mdb": {
        "16": "fileicons-office16 icon-mdb"
        , "32": "fileicons-office32 icon-mdb"
        , "48": "fileicons-office48 icon-mdb"
        , "96": "fileicons-office96-icon-mdb"
        , "256": "fileicons-office256-icon-mdb"
    }
    , "mht": {
        "16": "fileicons-office16 icon-mht"
        , "32": "fileicons-office32 icon-mht"
        , "48": "fileicons-office48 icon-mht"
        , "96": "fileicons-office96-icon-mht"
        , "256": "fileicons-office256-icon-mht"
    }
    , "mpp": {
        "16": "fileicons-office16 icon-mpp"
        , "32": "fileicons-office32 icon-mpp"
        , "48": "fileicons-office48 icon-mpp"
        , "96": "fileicons-office96-icon-mpp"
        , "256": "fileicons-office256-icon-mpp"
    }
    , "mpt": {
        "16": "fileicons-office16 icon-mpt"
        , "32": "fileicons-office32 icon-mpt"
        , "48": "fileicons-office48 icon-mpt"
        , "96": "fileicons-office96-icon-mpt"
        , "256": "fileicons-office256-icon-mpt"
    }
    , "msg": {
        "16": "fileicons-office16 icon-msg"
        , "32": "fileicons-office32 icon-msg"
        , "48": "fileicons-office48 icon-msg"
        , "96": "fileicons-office96-icon-msg"
        , "256": "fileicons-office256-icon-msg"
    }
    , "odp": {
        "16": "fileicons-office16 icon-odp"
        , "32": "fileicons-office32 icon-odp"
        , "48": "fileicons-office48 icon-odp"
        , "96": "fileicons-office96-icon-odp"
        , "256": "fileicons-office256-icon-odp"
    }
    , "ods": {
        "16": "fileicons-office16 icon-ods"
        , "32": "fileicons-office32 icon-ods"
        , "48": "fileicons-office48 icon-ods"
        , "96": "fileicons-office96-icon-ods"
        , "256": "fileicons-office256-icon-ods"
    }
    , "odt": {
        "16": "fileicons-office16 icon-odt"
        , "32": "fileicons-office32 icon-odt"
        , "48": "fileicons-office48 icon-odt"
        , "96": "fileicons-office96-icon-odt"
        , "256": "fileicons-office256-icon-odt"
    }
    , "pdf": {
        "16": "fileicons-office16 icon-pdf"
        , "32": "fileicons-office32 icon-pdf"
        , "48": "fileicons-office48 icon-pdf"
        , "96": "fileicons-office96-icon-pdf"
        , "256": "fileicons-office256-icon-pdf"
    }
    , "pps": {
        "16": "fileicons-office16 icon-pps"
        , "32": "fileicons-office32 icon-pps"
        , "48": "fileicons-office48 icon-pps"
        , "96": "fileicons-office96-icon-pps"
        , "256": "fileicons-office256-icon-pps"
    }
    , "ppsx": {
        "16": "fileicons-office16 icon-ppsx"
        , "32": "fileicons-office32 icon-ppsx"
        , "48": "fileicons-office48 icon-ppsx"
        , "96": "fileicons-office96-icon-ppsx"
        , "256": "fileicons-office256-icon-ppsx"
    }
    , "ppt": {
        "16": "fileicons-office16 icon-ppt"
        , "32": "fileicons-office32 icon-ppt"
        , "48": "fileicons-office48 icon-ppt"
        , "96": "fileicons-office96-icon-ppt"
        , "256": "fileicons-office256-icon-ppt"
    }
    , "pptx": {
        "16": "fileicons-office16 icon-pptx"
        , "32": "fileicons-office32 icon-pptx"
        , "48": "fileicons-office48 icon-pptx"
        , "96": "fileicons-office96-icon-pptx"
        , "256": "fileicons-office256-icon-pptx"
    }
    , "pst": {
        "16": "fileicons-office16 icon-pst"
        , "32": "fileicons-office32 icon-pst"
        , "48": "fileicons-office48 icon-pst"
        , "96": "fileicons-office96-icon-pst"
        , "256": "fileicons-office256-icon-pst"
    }
    , "pub": {
        "16": "fileicons-office16 icon-pub"
        , "32": "fileicons-office32 icon-pub"
        , "48": "fileicons-office48 icon-pub"
        , "96": "fileicons-office96-icon-pub"
        , "256": "fileicons-office256-icon-pub"
    }
    , "vcf": {
        "16": "fileicons-office16 icon-vcf"
        , "32": "fileicons-office32 icon-vcf"
        , "48": "fileicons-office48 icon-vcf"
        , "96": "fileicons-office96-icon-vcf"
        , "256": "fileicons-office256-icon-vcf"
    }
    , "vdw": {
        "16": "fileicons-office16 icon-vdw"
        , "32": "fileicons-office32 icon-vdw"
        , "48": "fileicons-office48 icon-vdw"
        , "96": "fileicons-office96-icon-vdw"
        , "256": "fileicons-office256-icon-vdw"
    }
    , "vdx": {
        "16": "fileicons-office16 icon-vdx"
        , "32": "fileicons-office32 icon-vdx"
        , "48": "fileicons-office48 icon-vdx"
        , "96": "fileicons-office96-icon-vdx"
        , "256": "fileicons-office256-icon-vdx"
    }
    , "vsd": {
        "16": "fileicons-office16 icon-vsd"
        , "32": "fileicons-office32 icon-vsd"
        , "48": "fileicons-office48 icon-vsd"
        , "96": "fileicons-office96-icon-vsd"
        , "256": "fileicons-office256-icon-vsd"
    }
    , "vsdx": {
        "16": "fileicons-office16 icon-vsdx"
        , "32": "fileicons-office32 icon-vsdx"
        , "48": "fileicons-office48 icon-vsdx"
        , "96": "fileicons-office96-icon-vsdx"
        , "256": "fileicons-office256-icon-vsdx"
    }
    , "vss": {
        "16": "fileicons-office16 icon-vss"
        , "32": "fileicons-office32 icon-vss"
        , "48": "fileicons-office48 icon-vss"
        , "96": "fileicons-office96-icon-vss"
        , "256": "fileicons-office256-icon-vss"
    }
    , "vst": {
        "16": "fileicons-office16 icon-vst"
        , "32": "fileicons-office32 icon-vst"
        , "48": "fileicons-office48 icon-vst"
        , "96": "fileicons-office96-icon-vst"
        , "256": "fileicons-office256-icon-vst"
    }
    , "vsx": {
        "16": "fileicons-office16 icon-vsx"
        , "32": "fileicons-office32 icon-vsx"
        , "48": "fileicons-office48 icon-vsx"
        , "96": "fileicons-office96-icon-vsx"
        , "256": "fileicons-office256-icon-vsx"
    }
    , "vtx": {
        "16": "fileicons-office16 icon-vtx"
        , "32": "fileicons-office32 icon-vtx"
        , "48": "fileicons-office48 icon-vtx"
        , "96": "fileicons-office96-icon-vtx"
        , "256": "fileicons-office256-icon-vtx"
    }
    , "xls": {
        "16": "fileicons-office16 icon-xls"
        , "32": "fileicons-office32 icon-xls"
        , "48": "fileicons-office48 icon-xls"
        , "96": "fileicons-office96-icon-xls"
        , "256": "fileicons-office256-icon-xls"
    }
    , "xlsb": {
        "16": "fileicons-office16 icon-xlsb"
        , "32": "fileicons-office32 icon-xlsb"
        , "48": "fileicons-office48 icon-xlsb"
        , "96": "fileicons-office96-icon-xlsb"
        , "256": "fileicons-office256-icon-xlsb"
    }
    , "xlsm": {
        "16": "fileicons-office16 icon-xlsm"
        , "32": "fileicons-office32 icon-xlsm"
        , "48": "fileicons-office48 icon-xlsm"
        , "96": "fileicons-office96-icon-xlsm"
        , "256": "fileicons-office256-icon-xlsm"
    }
    , "xlsx": {
        "16": "fileicons-office16 icon-xlsx"
        , "32": "fileicons-office32 icon-xlsx"
        , "48": "fileicons-office48 icon-xlsx"
        , "96": "fileicons-office96-icon-xlsx"
        , "256": "fileicons-office256-icon-xlsx"
    }
    , "xlt": {
        "16": "fileicons-office16 icon-xlt"
        , "32": "fileicons-office32 icon-xlt"
        , "48": "fileicons-office48 icon-xlt"
        , "96": "fileicons-office96-icon-xlt"
        , "256": "fileicons-office256-icon-xlt"
    }
    , "xltx": {
        "16": "fileicons-office16 icon-xltx"
        , "32": "fileicons-office32 icon-xltx"
        , "48": "fileicons-office48 icon-xltx"
        , "96": "fileicons-office96-icon-xltx"
        , "256": "fileicons-office256-icon-xltx"
    }
    , "xps": {
        "16": "fileicons-office16 icon-xps"
        , "32": "fileicons-office32 icon-xps"
        , "48": "fileicons-office48 icon-xps"
        , "96": "fileicons-office96-icon-xps"
        , "256": "fileicons-office256-icon-xps"
    }
    , "cab": {
        "16": "fileicons-system16 icon-cab"
        , "32": "fileicons-system32 icon-cab"
        , "48": "fileicons-system48 icon-cab"
        , "96": "fileicons-system96-icon-cab"
        , "256": "fileicons-system256-icon-cab"
    }
    , "cer": {
        "16": "fileicons-system16 icon-cer"
        , "32": "fileicons-system32 icon-cer"
        , "48": "fileicons-system48 icon-cer"
        , "96": "fileicons-system96-icon-cer"
        , "256": "fileicons-system256-icon-cer"
    }
    , "chm": {
        "16": "fileicons-system16 icon-chm"
        , "32": "fileicons-system32 icon-chm"
        , "48": "fileicons-system48 icon-chm"
        , "96": "fileicons-system96-icon-chm"
        , "256": "fileicons-system256-icon-chm"
    }
    , "cmd": {
        "16": "fileicons-system16 icon-cmd"
        , "32": "fileicons-system32 icon-cmd"
        , "48": "fileicons-system48 icon-cmd"
        , "96": "fileicons-system96-icon-cmd"
        , "256": "fileicons-system256-icon-cmd"
    }
    , "dll": {
        "16": "fileicons-system16 icon-dll"
        , "32": "fileicons-system32 icon-dll"
        , "48": "fileicons-system48 icon-dll"
        , "96": "fileicons-system96-icon-dll"
        , "256": "fileicons-system256-icon-dll"
    }
    , "exe": {
        "16": "fileicons-system16 icon-exe"
        , "32": "fileicons-system32 icon-exe"
        , "48": "fileicons-system48 icon-exe"
        , "96": "fileicons-system96-icon-exe"
        , "256": "fileicons-system256-icon-exe"
    }
    , "fon": {
        "16": "fileicons-system16 icon-fon"
        , "32": "fileicons-system32 icon-fon"
        , "48": "fileicons-system48 icon-fon"
        , "96": "fileicons-system96-icon-fon"
        , "256": "fileicons-system256-icon-fon"
    }
    , "hlp": {
        "16": "fileicons-system16 icon-hlp"
        , "32": "fileicons-system32 icon-hlp"
        , "48": "fileicons-system48 icon-hlp"
        , "96": "fileicons-system96-icon-hlp"
        , "256": "fileicons-system256-icon-hlp"
    }
    , "ini": {
        "16": "fileicons-system16 icon-ini"
        , "32": "fileicons-system32 icon-ini"
        , "48": "fileicons-system48 icon-ini"
        , "96": "fileicons-system96-icon-ini"
        , "256": "fileicons-system256-icon-ini"
    }
    , "lnk": {
        "16": "fileicons-system16 icon-lnk"
        , "32": "fileicons-system32 icon-lnk"
        , "48": "fileicons-system48 icon-lnk"
        , "96": "fileicons-system96-icon-lnk"
        , "256": "fileicons-system256-icon-lnk"
    }
    , "msi": {
        "16": "fileicons-system16 icon-msi"
        , "32": "fileicons-system32 icon-msi"
        , "48": "fileicons-system48 icon-msi"
        , "96": "fileicons-system96-icon-msi"
        , "256": "fileicons-system256-icon-msi"
    }
    , "ps1": {
        "16": "fileicons-system16 icon-ps1"
        , "32": "fileicons-system32 icon-ps1"
        , "48": "fileicons-system48 icon-ps1"
        , "96": "fileicons-system96-icon-ps1"
        , "256": "fileicons-system256-icon-ps1"
    }
    , "reg": {
        "16": "fileicons-system16 icon-reg"
        , "32": "fileicons-system32 icon-reg"
        , "48": "fileicons-system48 icon-reg"
        , "96": "fileicons-system96-icon-reg"
        , "256": "fileicons-system256-icon-reg"
    }
    , "rtf": {
        "16": "fileicons-system16 icon-rtf"
        , "32": "fileicons-system32 icon-rtf"
        , "48": "fileicons-system48 icon-rtf"
        , "96": "fileicons-system96-icon-rtf"
        , "256": "fileicons-system256-icon-rtf"
    }
    , "txt": {
        "16": "fileicons-system16 icon-txt"
        , "32": "fileicons-system32 icon-txt"
        , "48": "fileicons-system48 icon-txt"
        , "96": "fileicons-system96-icon-txt"
        , "256": "fileicons-system256-icon-txt"
    }
    , "vbs": {
        "16": "fileicons-system16 icon-vbs"
        , "32": "fileicons-system32 icon-vbs"
        , "48": "fileicons-system48 icon-vbs"
        , "96": "fileicons-system96-icon-vbs"
        , "256": "fileicons-system256-icon-vbs"
    }
    , "3gp": {
        "16": "fileicons-video16 icon-3gp"
        , "32": "fileicons-video32 icon-3gp"
        , "48": "fileicons-video48 icon-3gp"
        , "96": "fileicons-video96-icon-3gp"
        , "256": "fileicons-video256-icon-3gp"
    }
    , "avi": {
        "16": "fileicons-video16 icon-avi"
        , "32": "fileicons-video32 icon-avi"
        , "48": "fileicons-video48 icon-avi"
        , "96": "fileicons-video96-icon-avi"
        , "256": "fileicons-video256-icon-avi"
    }
    , "flv": {
        "16": "fileicons-video16 icon-flv"
        , "32": "fileicons-video32 icon-flv"
        , "48": "fileicons-video48 icon-flv"
        , "96": "fileicons-video96-icon-flv"
        , "256": "fileicons-video256-icon-flv"
    }
    , "m4v": {
        "16": "fileicons-video16 icon-m4v"
        , "32": "fileicons-video32 icon-m4v"
        , "48": "fileicons-video48 icon-m4v"
        , "96": "fileicons-video96-icon-m4v"
        , "256": "fileicons-video256-icon-m4v"
    }
    , "mkv": {
        "16": "fileicons-video16 icon-mkv"
        , "32": "fileicons-video32 icon-mkv"
        , "48": "fileicons-video48 icon-mkv"
        , "96": "fileicons-video96-icon-mkv"
        , "256": "fileicons-video256-icon-mkv"
    }
    , "mov": {
        "16": "fileicons-video16 icon-mov"
        , "32": "fileicons-video32 icon-mov"
        , "48": "fileicons-video48 icon-mov"
        , "96": "fileicons-video96-icon-mov"
        , "256": "fileicons-video256-icon-mov"
    }
    , "mp4": {
        "16": "fileicons-video16 icon-mp4"
        , "32": "fileicons-video32 icon-mp4"
        , "48": "fileicons-video48 icon-mp4"
        , "96": "fileicons-video96-icon-mp4"
        , "256": "fileicons-video256-icon-mp4"
    }
    , "mpg": {
        "16": "fileicons-video16 icon-mpg"
        , "32": "fileicons-video32 icon-mpg"
        , "48": "fileicons-video48 icon-mpg"
        , "96": "fileicons-video96-icon-mpg"
        , "256": "fileicons-video256-icon-mpg"
    }
    , "ogg": {
        "16": "fileicons-video16 icon-ogg"
        , "32": "fileicons-video32 icon-ogg"
        , "48": "fileicons-video48 icon-ogg"
        , "96": "fileicons-video96-icon-ogg"
        , "256": "fileicons-video256-icon-ogg"
    }
    , "webm": {
        "16": "fileicons-video16 icon-webm"
        , "32": "fileicons-video32 icon-webm"
        , "48": "fileicons-video48 icon-webm"
        , "96": "fileicons-video96-icon-webm"
        , "256": "fileicons-video256-icon-webm"
    }
    , "wmv": {
        "16": "fileicons-video16 icon-wmv"
        , "32": "fileicons-video32 icon-wmv"
        , "48": "fileicons-video48 icon-wmv"
        , "96": "fileicons-video96-icon-wmv"
        , "256": "fileicons-video256-icon-wmv"
    }
    , "asax": {
        "16": "fileicons-web16 icon-asax"
        , "32": "fileicons-web32 icon-asax"
        , "48": "fileicons-web48 icon-asax"
        , "96": "fileicons-web96-icon-asax"
        , "256": "fileicons-web256-icon-asax"
    }
    , "ascx": {
        "16": "fileicons-web16 icon-ascx"
        , "32": "fileicons-web32 icon-ascx"
        , "48": "fileicons-web48 icon-ascx"
        , "96": "fileicons-web96-icon-ascx"
        , "256": "fileicons-web256-icon-ascx"
    }
    , "ashx": {
        "16": "fileicons-web16 icon-ashx"
        , "32": "fileicons-web32 icon-ashx"
        , "48": "fileicons-web48 icon-ashx"
        , "96": "fileicons-web96-icon-ashx"
        , "256": "fileicons-web256-icon-ashx"
    }
    , "asmx": {
        "16": "fileicons-web16 icon-asmx"
        , "32": "fileicons-web32 icon-asmx"
        , "48": "fileicons-web48 icon-asmx"
        , "96": "fileicons-web96-icon-asmx"
        , "256": "fileicons-web256-icon-asmx"
    }
    , "aspx": {
        "16": "fileicons-web16 icon-aspx"
        , "32": "fileicons-web32 icon-aspx"
        , "48": "fileicons-web48 icon-aspx"
        , "96": "fileicons-web96-icon-aspx"
        , "256": "fileicons-web256-icon-aspx"
    }
    , "config": {
        "16": "fileicons-web16 icon-config"
        , "32": "fileicons-web32 icon-config"
        , "48": "fileicons-web48 icon-config"
        , "96": "fileicons-web96-icon-config"
        , "256": "fileicons-web256-icon-config"
    }
    , "cshtml": {
        "16": "fileicons-web16 icon-cshtml"
        , "32": "fileicons-web32 icon-cshtml"
        , "48": "fileicons-web48 icon-cshtml"
        , "96": "fileicons-web96-icon-cshtml"
        , "256": "fileicons-web256-icon-cshtml"
    }
    , "css": {
        "16": "fileicons-web16 icon-css"
        , "32": "fileicons-web32 icon-css"
        , "48": "fileicons-web48 icon-css"
        , "96": "fileicons-web96-icon-css"
        , "256": "fileicons-web256-icon-css"
    }
    , "htm": {
        "16": "fileicons-web16 icon-htm"
        , "32": "fileicons-web32 icon-htm"
        , "48": "fileicons-web48 icon-htm"
        , "96": "fileicons-web96-icon-htm"
        , "256": "fileicons-web256-icon-htm"
    }
    , "js": {
        "16": "fileicons-web16 icon-js"
        , "32": "fileicons-web32 icon-js"
        , "48": "fileicons-web48 icon-js"
        , "96": "fileicons-web96-icon-js"
        , "256": "fileicons-web256-icon-js"
    }
    , "swf": {
        "16": "fileicons-web16 icon-swf"
        , "32": "fileicons-web32 icon-swf"
        , "48": "fileicons-web48 icon-swf"
        , "96": "fileicons-web96-icon-swf"
        , "256": "fileicons-web256-icon-swf"
    }
    , "xap": {
        "16": "fileicons-web16 icon-xap"
        , "32": "fileicons-web32 icon-xap"
        , "48": "fileicons-web48 icon-xap"
        , "96": "fileicons-web96-icon-xap"
        , "256": "fileicons-web256-icon-xap"
    }
};
GleamTech.FileUltimate.FileTypeInfo.fileIconMappings = {
    "tgz": "tar"
    , "taz": "tar"
    , "tbz": "tar"
    , "tbz2": "tar"
    , "tz2": "tar"
    , "tlz": "tar"
    , "txz": "tar"
    , "sit": "sitx"
    , "f4a": "m4a"
    , "oga": "ogg"
    , "mid": "midi"
    , "vorbis": "aac"
    , "aif": "aac"
    , "aiff": "aac"
    , "aifc": "aac"
    , "flac": "aac"
    , "m4p": "aac"
    , "max": "3ds"
    , "sdf": "mdf"
    , "user": "suo"
    , "xslt": "xsl"
    , "tif": "bmp"
    , "tiff": "bmp"
    , "tff": "bmp"
    , "tga": "bmp"
    , "webp": "bmp"
    , "dcm": "bmp"
    , "djvu": "bmp"
    , "emf": "bmp"
    , "wmf": "bmp"
    , "dib": "bmp"
    , "jpeg": "jpg"
    , "jpe": "jpg"
    , "jif": "jpg"
    , "jfif": "jpg"
    , "jfi": "jpg"
    , "exif": "jpg"
    , "jp2": "jpg"
    , "jpf": "jpg"
    , "jpx": "jpg"
    , "j2k": "jpg"
    , "j2c": "jpg"
    , "jpc": "jpg"
    , "jxr": "jpg"
    , "wdp": "jpg"
    , "hdp": "jpg"
    , "ott": "odt"
    , "ots": "ods"
    , "otp": "odp"
    , "eml": "msg"
    , "emlx": "msg"
    , "mhtml": "mht"
    , "com": "exe"
    , "sys": "dll"
    , "ocx": "dll"
    , "cpl": "dll"
    , "bat": "cmd"
    , "inf": "ini"
    , "otf": "fon"
    , "ttf": "fon"
    , "log": "txt"
    , "crt": "cer"
    , "mp4v": "mp4"
    , "f4v": "mp4"
    , "3g2": "3gp"
    , "mpe": "mpg"
    , "mpeg": "mpg"
    , "vob": "mpg"
    , "asf": "webm"
    , "m2ts": "webm"
    , "mts": "webm"
    , "ogv": "ogg"
    , "master": "aspx"
    , "html": "htm"
    , "vbhtml": "cshtml"
};
GleamTech.FileUltimate.FileTypeInfo.thumbnailTypes = {
    "jpg": {
        "isImage": true
    }
    , "jpeg": {
        "isImage": true
    }
    , "jpe": {
        "isImage": true
    }
    , "jif": {
        "isImage": true
    }
    , "exif": {
        "isImage": true
    }
    , "jfi": {
        "isImage": true
    }
    , "jfif": {
        "isImage": true
    }
    , "jp2": {
        "isImage": true
    }
    , "jpc": {
        "isImage": true
    }
    , "j2k": {
        "isImage": true
    }
    , "jpx": {
        "isImage": true
    }
    , "jpf": {
        "isImage": true
    }
    , "j2c": {
        "isImage": true
    }
    , "png": {
        "isImage": true
    }
    , "gif": {
        "isImage": true
    }
    , "webp": {
        "isImage": true
    }
    , "bmp": {
        "isImage": true
    }
    , "wmf": {
        "isImage": true
    }
    , "dib": {
        "isImage": true
    }
    , "ico": {
        "isImage": true
    }
    , "tga": {
        "isImage": true
    }
    , "psd": {
        "isImage": true
    }
    , "psb": {
        "isImage": true
    }
    , "tif": {
        "isImage": true
    }
    , "tiff": {
        "isImage": true
    }
    , "tff": {
        "isImage": true
    }
    , "svg": {
        "isImage": true
    }
    , "emf": {
        "isImage": true
    }
    , "dng": {
        "isImage": true
    }
    , "arw": {
        "isImage": true
    }
    , "sr2": {
        "isImage": true
    }
    , "srf": {
        "isImage": true
    }
    , "cr2": {
        "isImage": true
    }
    , "crw": {
        "isImage": true
    }
    , "nef": {
        "isImage": true
    }
    , "nrw": {
        "isImage": true
    }
    , "raf": {
        "isImage": true
    }
    , "orf": {
        "isImage": true
    }
    , "pef": {
        "isImage": true
    }
    , "kdc": {
        "isImage": true
    }
    , "dcr": {
        "isImage": true
    }
    , "mrw": {
        "isImage": true
    }
    , "erf": {
        "isImage": true
    }
    , "rw2": {
        "isImage": true
    }
    , "srw": {
        "isImage": true
    }
    , "x3f": {
        "isImage": true
    }
    , "mef": {
        "isImage": true
    }
    , "raw": {
        "isImage": true
    }
    , "avi": {
        "isVideo": true
    }
    , "mp4": {
        "isVideo": true
    }
    , "m4v": {
        "isVideo": true
    }
    , "mp4v": {
        "isVideo": true
    }
    , "3g2": {
        "isVideo": true
    }
    , "3gp": {
        "isVideo": true
    }
    , "mpg": {
        "isVideo": true
    }
    , "mpeg": {
        "isVideo": true
    }
    , "mpe": {
        "isVideo": true
    }
    , "vob": {
        "isVideo": true
    }
    , "mov": {
        "isVideo": true
    }
    , "mkv": {
        "isVideo": true
    }
    , "wmv": {
        "isVideo": true
    }
    , "asf": {
        "isVideo": true
    }
    , "m2ts": {
        "isVideo": true
    }
    , "mts": {
        "isVideo": true
    }
    , "ts": {
        "isVideo": true
    }
    , "m2t": {
        "isVideo": true
    }
    , "flv": {
        "isVideo": true
    }
    , "f4v": {
        "isVideo": true
    }
    , "webm": {
        "isVideo": true
    }
    , "ogv": {
        "isVideo": true
    }
};
GleamTech.FileUltimate.FileTypeInfo.archiveTypes = {
    "zip": {
        "isReadWrite": true
    }
    , "7z": {
        "isReadOnly": true
    }
    , "rar": {
        "isReadOnly": true
    }
    , "tar": {
        "isReadOnly": true
    }
    , "tgz": {
        "isReadOnly": true
    }
    , "taz": {
        "isReadOnly": true
    }
    , "tbz": {
        "isReadOnly": true
    }
    , "tbz2": {
        "isReadOnly": true
    }
    , "tz2": {
        "isReadOnly": true
    }
    , "tlz": {
        "isReadOnly": true
    }
    , "txz": {
        "isReadOnly": true
    }
    , "gz": {
        "isReadOnly": true
    }
    , "gzip": {
        "isReadOnly": true
    }
    , "bz2": {
        "isReadOnly": true
    }
    , "bzip2": {
        "isReadOnly": true
    }
    , "lz": {
        "isReadOnly": true
    }
    , "lzip": {
        "isReadOnly": true
    }
    , "xz": {
        "isReadOnly": true
    }
};
GleamTech.FileUltimate.FileTypeInfo.imageViewerTypes = {
    "jpg": {}
    , "jpeg": {}
    , "jpe": {}
    , "jif": {}
    , "exif": {}
    , "jfi": {}
    , "jfif": {}
    , "jp2": {}
    , "jpc": {}
    , "j2k": {}
    , "jpx": {}
    , "jpf": {}
    , "j2c": {}
    , "png": {}
    , "gif": {}
    , "webp": {}
    , "bmp": {}
    , "wmf": {}
    , "dib": {}
    , "ico": {}
    , "tga": {}
    , "psd": {}
    , "psb": {}
    , "tif": {
        "excluded": true
    }
    , "tiff": {
        "excluded": true
    }
    , "tff": {
        "excluded": true
    }
    , "svg": {}
    , "emf": {}
    , "dng": {}
    , "arw": {}
    , "sr2": {}
    , "srf": {}
    , "cr2": {}
    , "crw": {}
    , "nef": {}
    , "nrw": {}
    , "raf": {}
    , "orf": {}
    , "pef": {}
    , "kdc": {}
    , "dcr": {}
    , "mrw": {}
    , "erf": {}
    , "rw2": {}
    , "srw": {}
    , "x3f": {}
    , "mef": {}
    , "raw": {}
};
GleamTech.FileUltimate.FileTypeInfo.documentViewerTypes = {
    "pdf": {}
    , "xps": {}
    , "oxps": {}
    , "xpz": {}
    , "docx": {}
    , "docm": {}
    , "dotx": {}
    , "dotm": {}
    , "doc": {}
    , "dot": {}
    , "rtf": {}
    , "odt": {}
    , "ott": {}
    , "xlsx": {}
    , "xlsm": {}
    , "xltx": {}
    , "xltm": {}
    , "xlam": {}
    , "xlsb": {}
    , "xls": {}
    , "xlt": {}
    , "csv": {}
    , "tsv": {}
    , "ods": {}
    , "ots": {}
    , "pptx": {}
    , "pptm": {}
    , "potx": {}
    , "potm": {}
    , "ppsx": {}
    , "ppsm": {}
    , "ppt": {}
    , "pps": {}
    , "odp": {}
    , "otp": {}
    , "vsdx": {}
    , "vsdm": {}
    , "vstx": {}
    , "vstm": {}
    , "vssx": {}
    , "vssm": {}
    , "vdx": {}
    , "vsx": {}
    , "vtx": {}
    , "vsd": {}
    , "vss": {}
    , "vst": {}
    , "vdw": {}
    , "mpp": {}
    , "mpt": {}
    , "msg": {}
    , "eml": {}
    , "emlx": {}
    , "html": {
        "excluded": true
    }
    , "htm": {
        "excluded": true
    }
    , "mht": {}
    , "mhtml": {}
    , "txt": {
        "excluded": true
    }
    , "xml": {
        "excluded": true
    }
    , "dwg": {}
    , "dxf": {}
    , "tif": {}
    , "tiff": {}
    , "djvu": {}
    , "dcm": {}
    , "svg": {}
    , "emf": {}
    , "psd": {}
    , "jpg": {}
    , "jpeg": {}
    , "jpe": {}
    , "jfif": {}
    , "jp2": {}
    , "jpf": {}
    , "jpx": {}
    , "j2k": {}
    , "j2c": {}
    , "jpc": {}
    , "jxr": {}
    , "wdp": {}
    , "hdp": {}
    , "png": {}
    , "gif": {}
    , "webp": {}
    , "bmp": {}
    , "wmf": {}
    , "dib": {}
};
