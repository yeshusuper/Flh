 $(function () {
     function upload_check(filepath, _this) {
            var byteSize = _this.files[0].size;
            if (!/.(gif|jpg|jpeg|png|bmp)$/.test(filepath)) {
                alert('invoice_cover_main', '仅支持 .jpg .jpeg .bmp .gif .png格式照片！');
                return false;
            } else if (byteSize > 2 * 1024 * 1024) {
                alert('invoice_cover_main', '图片大小不超过2M！');
                return false;
            }
        }
    //上传
    $('.file_upload').live('change', function () {
            var filepath = $(this).val(),
                _this = this,
                type_data = $(this).data('type'),
                ileObj = $(this).get(0).files;
            upload_check(filepath, _this);
            $.ajaxFileUpload({
                url: "/Image/UploadTemp",
                secureuri: false,
                fileElementId: "file",//file标签的id  
                dataType: 'json',
                success: function (data, status) {
                    if (typeof data == "string") {
                        data = JSON.parse(data);
                    }
                    var html=$(".product-hide-tr").html();
                    $('.product-edit-bottom').before('<tr class="product-edit-tr">'+html+'</tr>');
                    var obj=$('.common-list-table .product-edit-tr:last');
                    $('.list_num',obj).text($('.product-edit-tr').length)
                    $('[name="classNo"]',obj).val(classNo);
                    $('[name="imagePath"]',obj).val(data.data.id );
                    $('img',obj).attr('src','http://img.fuliaohui.com/' + data.data.id + '?x-oss-process=style/product-list');
                },
                error: function (data, status, e) {
                    //alert(e);
                }
            });
        })
    //触发图片上传
    $('.upload-img').on('click', function () {
            $('.file_upload').click()
        })

    if(pids && pids!=""){
        var num=1;
        $.ajax({
            type: "post",
            url:"/Product/BatchEditList",
            data: { "pids": pids },
            dataType: "json",
            traditional: true,
            success: function (res) {
               $.each(res,function(key,item){
                    $('.upload-img').hide();
                    var html=$(".product-hide-tr").html();
                    $('.product-edit-bottom').before('<tr class="product-edit-tr">'+html+'</tr>');
                    var obj=$('.common-list-table .product-edit-tr:last');
                    $('.list_num',obj).text(num++)
                    $('[name="pid"]',obj).val(item.pid);
                    $('[name="name"]',obj).val(item.name);
                    $('[name="enName"]',obj).val(item.enName);
                    $('[name="description"]',obj).val(item.description);
                    $('[name="enDescription"]',obj).val(item.enDescription);
                    $('[name="size"]',obj).val(item.size);
                    $('[name="enSize"]',obj).val(item.enSize);
                    $('[name="color"]',obj).val(item.color);
                    $('[name="enColor"]',obj).val(item.enColor);
                    $('[name="material"]',obj).val(item.material);
                    $('[name="enMaterial"]',obj).val(item.enMaterial);
                    $('[name="technique"]',obj).val(item.technique);
                    $('[name="enTechnique"]',obj).val(item.enTechnique);
                    $('[name="minQuantity"]',obj).val(item.minQuantity);
                    $('[name="deliveryDay"]',obj).val(item.deliveryDay);
                    $('[name="keywords"]',obj).val(item.keywords);
                    $('[name="enKeywords"]',obj).val(item.enKeywords);
                    $('[name="unitPrice"]',obj).val(item.unitPrice);
                    $('[name="sortNo"]',obj).val(item.sortNo);
                    $('[name="classNo"]',obj).val(item.classNo);
                    $('[name="imagePath"]',obj).val(item.imagePath);
                    $('img',obj).attr('src',item.imagePath);
               })
           }
        });
    }

    //把选中的第一个值应用到所有项中
    $('.fun-insert').on('click',function(){
        var ochecked=$('.product-edit-th input:checked'),
            olisr=$('.product-edit-tr'),
            olist_first=$('.product-edit-tr:first');
        $.each(ochecked,function(key,item){
            var name=$(item).attr('name'),
                nameALL=$('[name="'+name+'"]',olisr),
                ennameALL=$('[name="en'+name+'"]',olisr);
            $.each(nameALL,function(key_c,item_c){
                if($(item_c).val()==""){
                    alert($('[name="'+name+'"]',olist_first).val())
                    $(item_c).val($('[name="'+name+'"]',olist_first).val());
                }
            })
            $.each(ennameALL,function(key_e,item_e){
                if($(item_e).val()==""){
                    $(item_e).val($('[name="en'+name+'"]',olist_first).val());
                }
            })
        })
    })
        
    //保存
    $('.fun-save').on('click', function () {
        var _this=$(this);
        if(_this.text()!='保存'){
            return false;
        }
        var productObj=$('.product-edit-tr'),
            data_arr=[];
            if(productObj.length>0){
                $.each(productObj,function(key,item){
                    var $current=$(item),
                        inputAll=$('input',$current),
                        textareaAll=$('textarea',$current),
                        data_obj={};
                    $.each(inputAll,function(key_i,item_i){
                        var name=$(item_i).attr('name'),
                            value=$(item_i).val();
                        data_obj[name]=value;
                    })
                    $.each(textareaAll,function(key_t,item_t){
                        var tname=$(item_t).attr('name'),
                            tvalue=$(item_t).val();
                        data_obj[tname]=tvalue;
                    })
                    data_arr.push(data_obj);
                });
                var remind="",
                    list_num=0;
                for(var i=0;i<data_arr.length;i++){
                    list_num++
                    var arr_obj=data_arr[i],
                        serror="";
                    if(!arr_obj.name || arr_obj.name==""){
                        serror+='产品名称不能空,';
                    }
                    if(!arr_obj.description || arr_obj.description==""){
                        serror+='产品详细说明不能空,';
                    }
                    if(!arr_obj.size || arr_obj.size==""){
                        serror+='产品尺寸不能空,';
                    }
                    if(!arr_obj.color || arr_obj.color==""){
                        serror+='产品颜色不能空,';
                    }
                    if(!arr_obj.material || arr_obj.material==""){
                        serror+='产品材质不能空,';
                    }
                    if(!arr_obj.technique || arr_obj.technique==""){
                        serror+='产品工艺不能空,';
                    }
                    if(!arr_obj.minQuantity || arr_obj.minQuantity==""){
                        serror+='产品起订量不能空,';
                    }
                    if(!arr_obj.deliveryDay || arr_obj.deliveryDay==""){
                        serror+='产品发货日不能空,';
                    }
                    if(!arr_obj.keywords || arr_obj.keywords==""){
                        serror+='产品关键字不能空,';
                    }
                    if(!arr_obj.unitPrice || arr_obj.unitPrice==""){
                        serror+='产品单价不能空,';
                    }
                    if(!arr_obj.sortNo || arr_obj.sortNo==""){
                        serror+='产品排序不能空。';
                    }
                    if(serror!=""){
                        serror='序号'+list_num+':'+serror+'\n';
                        remind+=serror;
                    } 
                }
                if(remind && remind!=""){
                   alert(remind);
                }else{
                    data_arr=JSON.stringify(data_arr);
                    _this.text('保存中...')
                    $.ajax({
                        type: "post",
                        url:"/product/SaveBatchEdit",
                        data: {"models":data_arr},
                        dataType: "json",
                        traditional: true,
                        success: function (res) {
                            //_this.text('保存');
                            //alert('保存成功！');
                             window.location.href="/product/list";
                        }
                    });
                }
            }else{
                alert("请上传图片后编辑产品信息再保存，谢谢。")
            }
        })
})