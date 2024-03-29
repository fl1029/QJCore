﻿var app = new Vue({
    el: '#DATABI_YBZZ',
    components: {
        'qjDate': httpVueLoader('../../AppPage/FORMBI/vue/qjDate.vue'),
        'qjInput': httpVueLoader('../../AppPage/FORMBI/vue/qjInput.vue'),
        'qjSN': httpVueLoader('../../AppPage/FORMBI/vue/qjSN.vue'),
        'qjInputNum': httpVueLoader('../../AppPage/FORMBI/vue/qjInputNum.vue'),
        'qjSelect': httpVueLoader('../../AppPage/FORMBI/vue/qjSelect.vue?v=2'),
        'qjMSelect': httpVueLoader('../../AppPage/FORMBI/vue/qjMSelect.vue'),
        'qjSelbranch': httpVueLoader('../../AppPage/FORMBI/vue/qjSelbranch.vue'),
        'qjSeluser': httpVueLoader('../../AppPage/FORMBI/vue/qjSeluser.vue'),
        'qjLine': httpVueLoader('../../AppPage/FORMBI/vue/qjLine.vue'),
        'qjTable': httpVueLoader('../../AppPage/FORMBI/vue/qjTable.vue'),
        'qjTab': httpVueLoader('../../AppPage/FORMBI/vue/qjTab.vue'),
        'qjEdit': httpVueLoader('../../AppPage/FORMBI/vue/qjEdit.vue'),
        'qjFile': httpVueLoader('../../AppPage/FORMBI/vue/qjFile.vue')
    },
    data: {
        vtype: ComFunJS.getQueryString('vtype', '0'),//0是默认单页模式,1是移动模式,2是弹框模式
        pdid: ComFunJS.getQueryString('pdid', '0'),
        iscopy: ComFunJS.getQueryString('iscopy', 'n'),
        FormCode: "BDGL",
        pddata: {},
        pidata: {},
        poption: { width: '0' },
        FormFile: [],
        loading: true,
        FormDecVisible: false,
        bodyopacity: 95,
        formtatus: "0",
        tabtype: "0",
        isview: true,//是否浏览模式,判断需不需要加载默认值
        nowwidget: {},
        wfdata: [],
        NoValData: [],
        readySize: 0,//组件加载数据
        FormData: {
            wigetitems: [],
            CustomCmponent: {}
        },
        draftdatas: [],
        nowwidget: {
        },
    },
    computed: {
        // 计算属性的 getter
        direction: function () {
            return this.vtype == "0" ? "horizontal" : "horizontal";
        }
    },
    methods: {
        addchildwig() {
            this.readySize++
            // 检查进度是否设置的colSize一致
            if (this.readySize == this.FormData.wigetitems.length) {
                this.layout();
            }
        },
        layout: function () {
            app.FormData.wigetitems.forEach(function (wig) {
                if (wig.component == "qjTab") {
                    wig.childpro.Tabs.forEach((tab, index) => {
                        tab.content.forEach((tabitem, indexitem) => {
                            $("#" + wig.wigdetcode).find(".el-tab-pane").eq(index).append($("#" + tabitem.wigdetcode));
                        })
                    })
                }
            })
        },
        zdh: function () {
            $(".containerb").toggleClass("widthmax");
            $(".elmain").toggleClass("pd40");
            $(".elmain").toggleClass("pd0");
        },
        changeop: function (val) {
            $("body").css("opacity", "." + val);
            localStorage.setItem("fopacity", val);
        },
        qp: function () {
            if (document.isFullScreen || document.mozIsFullScreen || document.webkitIsFullScreen) {
                ComFunJS.exitFullscreen()
            } else {
                ComFunJS.requestFullScreen();
            }
        },
        getfileurl: function (fileid) {
            return ComFunJS.getfile(fileid);
        },
        ChangeStatus: function () {
            location.reload();
        },
        GoForm: function () {
            location.href = "/ViewV5/AppPage/FORMBI/FormManage.html?vtype=" + app.vtype + "&piid=" + app.pidata.ID;
        },
        SaveExData: function (dataid) {
            //保存数据利于统计
            $.getJSON("/api/Bll/ExeAction?Action=FORMBI_SAVEEXDATA", { P1: dataid, P2: app.pdid }, function (result) {

            });
        },
        datachange: function (chidata, wigdetcode) {
            if (wigdetcode) {
                _.forEach(this.FormData.wigetitems, function (item) {
                    if (item.wigdetcode == wigdetcode) {
                        item.childpro = JSON.parse(chidata)
                    }
                })
            }
            else {
                app.nowwidget.childpro = JSON.parse(chidata);
            }
        },
        StarForm: function () {
            app.$refs.form.validate(function (boolean, object) {
                if (app.NoValData.length > 0) {
                    app.$refs.form.clearValidate(app.NoValData);
                }//遇到不需要验证得,先去除验证
                if (_.difference(_.keys(object), app.NoValData).length == 0) {
                    app.loading = true;
                    $.getJSON("/api/Bll/ExeAction?Action=FORMBI_STARTWF", { P1: app.FormCode, PDID: app.pdid, csr: "", zsr: "", content: JSON.stringify(app.FormData.wigetitems) }, function (result) {
                        if (!result.ErrorMsg) {
                            app.$notify({
                                title: '成功',
                                message: '发起表单成功',
                                type: 'success'
                            });
                            app.loading = false;
                            app.formtatus = "1";//成功状态
                            app.$refs.form.resetFields();
                            app.pidata = result.Result;
                            if (app.pddata.ProcessType == "-1") {
                                //没有流程得时候保存数据到扩展表或者关联表里
                                app.SaveExData(result.Result.ID);
                            }

                        }
                    });

                }
            })
        },
        SaveDraft: function () {

            var draftdata = { "ID": "0", "FormCode": app.FormCode, "FormID": app.pdid, "JsonData": "" };
            //app.FormData.CustomCmponent = this.$refs.CustomCmponent.GetData();
            draftdata.JsonData = JSON.stringify(app.FormData);
            $.getJSON("/api/Auth/ExeAction?Action=SAVEDRAFT", { P1: JSON.stringify(draftdata) }, function (result) {
                if (!result.ErrorMsg) {
                    app.draftdatas.push(result.Result);
                    app.$notify({
                        title: '成功',
                        message: '存草稿成功',
                        type: 'success'
                    });
                }
            })

        },
        DelDraft: function (item, index) {
            $.getJSON("/api/Auth/ExeAction?Action=DELDRAFT", { P1: item.ID }, function (result) {
                if (!result.ErrorMsg) {
                    app.draftdatas.splice(index, 1);

                    app.$notify({
                        title: '成功',
                        message: '删除草稿成功',
                        type: 'success'
                    });
                }
            })
        },
        SelDraft: function (item) {
            app.FormData.wigetitems = [];
            app.FormData = JSON.parse(item.JsonData);
            // this.$refs.CustomCmponent.SelDraft(app.FormData.CustomCmponent);

        },
        GetDraft: function () {
            $.getJSON("/api/Auth/ExeAction?Action=GETDRAFT", { P1: app.FormCode, P2: app.pdid }, function (r) {
                if (!r.ErrorMsg) {
                    app.draftdatas = r.Result;
                }

            })
        },
        InitWF: function () {
            $.getJSON("/api/Bll/ExeAction?Action=FORMBI_GETWFDATA", { P1: app.pdid }, function (result) {
                if (result.ErrorMsg == "") {
                    app.loading = false;
                    app.wfdata = result.Result;
                    app.pddata = result.Result1;
                    app.FormFile = result.Result2;
                    if (app.pddata.Poption) {
                        app.poption = JSON.parse(app.pddata.Poption);
                    }
                    if (app.pddata.Tempcontent) {
                        app.FormData.wigetitems = JSON.parse(app.pddata.Tempcontent);
                        //app.mangewigdet();
                        if (app.iscopy == 'y') {
                            var copydata = JSON.parse(localStorage.getItem("copydata"));
                            app.FormData.wigetitems = copydata.wigetitems;
                        }

                    }
                    if (app.pddata.ProcessType != "-1") {
                        var writefiled = app.wfdata[0].WritableFields.split(',');
                        var intindex = 0;
                        _.forEach(app.FormData.wigetitems, function (wiget) {
                            if (wiget.childpro.hasOwnProperty('disabled')) {
                                wiget.childpro.disabled = _.indexOf(writefiled, wiget.wigdetcode) == -1;
                            }
                            if (wiget.childpro.disabled) {
                              //  app.NoValData.push("wigetitems." + intindex + ".value")
                            }//具有权限才能编辑,否则就禁用,并去掉验证
                            intindex++;
                        })
                    }

                }
            })
        }

    },
    mounted: function () {
        var pro = this;
        pro.$nextTick(function () {
            if (!ComFunJS.isPC()) {
                pro.vtype = "1";
                $("body").addClass("mob")
            }
            pro.InitWF();
            pro.GetDraft();
            if (pro.vtype != "0") {
                pro.zdh();
            }
            if (pro.vtype == "0") {
                //PC模式添加背景
                var skinurl = "/ViewV5/images/skin/skin1.png";
                if (localStorage.getItem("fskin")) {
                    skinurl = localStorage.getItem("fskin");
                }
                $("body").css("background-image", "url('" + skinurl + "')");
                $(".skintp").bind('click', function (event) {
                    localStorage.setItem("fskin", $(this).attr("src"));
                    $("body").css("background-image", "url('" + $(this).attr("src") + "')");
                })

                var opacity = "97";
                if (localStorage.getItem("fopacity")) {
                    opacity = localStorage.getItem("fopacity");
                }
                $("body").css("opacity", "." + opacity);
            }
            if (pro.vtype == "2") {
                $("body").removeClass("BG")
                $("#FormFoot").addClass("FormFoot");
                //弹框模式,添加底部按钮样式,去除背景
            }

        })
    },
    created() {
        document.body.removeChild(document.getElementById('Loading'))

        var divBJ = document.getElementById('DATABI_YBZZ');
        divBJ.style.display = "block";
    }

})
