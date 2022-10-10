///擴充功能
//左側補字
String.prototype.PadLeft = function (lenght, char) {
    var s = this + '';
    return new Array(lenght - s.length + 1).join(char, '') + s;
}
//右側補字
String.prototype.PadRight = function (lenght, char) {
    var s = this + '';
    return s + new Array(lenght - s.length + 1).join(char, '');
}

var ReceiptResult = [];
$(function () {
    $("#InputNumber").bind('keydown', function (e) {
        if (e.which == 13) {
            $("#btn_CheckReceipt").click();
        }
    });


    success = function (Data) {
        if (Data.status) {
            ReceiptResult = Data.data;
            $.each(Data.data, function (index, data) {
                var AreaClass = (index == 0) ? ".New" :".Previous";
                
                $(AreaClass + " .ReceiptYear").html(data.receiptYear);
                $(AreaClass + " .ReceiptMonth").html(data.receiptMonth);
                $(AreaClass + " .SpecialNumber .Result").html("<font class='RedFont'>" + data.specialNumber + "</font>");
                $(AreaClass + " .SpecialNumber .Mark .RedFont").eq(0).html(data.specialDigit);
                $(AreaClass + " .SpecialNumber .Mark .RedFont").eq(1).html((data.specialPrize / 10000).toString() + "萬");
                $(AreaClass + " .GrandNumber .Result").html("<font class='RedFont'>" + data.grandNumber + "</font>");
                $(AreaClass + " .GrandNumber .Mark .RedFont").eq(0).html(data.grandDigit);
                $(AreaClass + " .GrandNumber .Mark .RedFont").eq(1).html((data.grandPrize / 10000).toString() + "萬");
                $(AreaClass + " .FirstNumber .Result").eq(0).html("<font>" + data.firstNumber_1.substr(0, 5) + "</font><font class='RedFont'>" + data.firstNumber_1.substr(5, 3) + "</font>");
                $(AreaClass + " .FirstNumber .Result").eq(1).html("<font>" + data.firstNumber_2.substr(0, 5) + "</font><font class='RedFont'>" + data.firstNumber_2.substr(5, 3) + "</font>");
                $(AreaClass + " .FirstNumber .Result").eq(2).html("<font>" + data.firstNumber_3.substr(0, 5) + "</font><font class='RedFont'>" + data.firstNumber_3.substr(5, 3) + "</font>");

                $(AreaClass + " .FirstNumber .Mark").eq(0).find(".RedFont").eq(0).html(data.firstDigit);
                $(AreaClass + " .FirstNumber .Mark").eq(0).find(".RedFont").eq(1).html((data.firstPrize / 10000).toString() + "萬");
                $(AreaClass + " .FirstNumber .Mark").eq(1).find(".RedFont").eq(0).html(data.secondDigit);
                $(AreaClass + " .FirstNumber .Mark").eq(1).find(".RedFont").eq(1).html((data.secondPrize / 10000).toString() + "萬");
                $(AreaClass + " .FirstNumber .Mark").eq(2).find(".RedFont").eq(0).html(data.thirdDigit);
                $(AreaClass + " .FirstNumber .Mark").eq(2).find(".RedFont").eq(1).html((data.thirdPrize / 10000).toString() + "萬");
                $(AreaClass + " .FirstNumber .Mark").eq(3).find(".RedFont").eq(0).html(data.fourthDigit);
                $(AreaClass + " .FirstNumber .Mark").eq(3).find(".RedFont").eq(1).html((data.fourthPrize / 1000).toString() + "千");
                $(AreaClass + " .FirstNumber .Mark").eq(4).find(".RedFont").eq(0).html(data.fifthDigit);
                $(AreaClass + " .FirstNumber .Mark").eq(4).find(".RedFont").eq(1).html((data.fifthPrize / 1000).toString() + "千");
                $(AreaClass + " .FirstNumber .Mark").eq(5).find(".RedFont").eq(0).html(data.sixthDigit);
                $(AreaClass + " .FirstNumber .Mark").eq(5).find(".RedFont").eq(1).html((data.sixthPrize / 100).toString() + "百");

                $("#InputNumber").focus();
            });
        } else {
            alert("數據異常:" + Data.errorMessage);
        }
    };
    var apiUrl = "https://localhost:44312/Receipt/GetReceiptPrize";
    CallApi(apiUrl, success);
});


function CheckedReceipt(Number) {
    Number = Number.PadLeft(8, '0');
    $.each(ReceiptResult, function (index, data) {
        var Issue = data.receiptYear + " 年 " + data.receiptMonth + " 月 "
        if (Number == data.specialNumber) {
            alert("恭喜中獎! " + Issue + " 特別獎: " + data.specialPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number == data.grandNumber) {
            alert("恭喜中獎! " + Issue + " 特獎: " + data.grandNumber + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number == data.firstNumber_1
            || Number == data.firstNumber_2
            || Number == data.firstNumber_3) {
            alert("恭喜中獎! " + Issue + " 頭獎: " + data.firstPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number.substr(1) == data.firstNumber_1.substr(1)
            || Number.substr(1) == data.firstNumber_2.substr(1)
            || Number.substr(1) == data.firstNumber_3.substr(1)) {
            alert("恭喜中獎! " + Issue + " 二獎: " + data.secondPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number.substr(2) == data.firstNumber_1.substr(2)
            || Number.substr(2) == data.firstNumber_2.substr(2)
            || Number.substr(2) == data.firstNumber_3.substr(2)) {
            alert("恭喜中獎! " + Issue + " 三獎: " + data.thirdPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number.substr(3) == data.firstNumber_1.substr(3)
            || Number.substr(3) == data.firstNumber_2.substr(3)
            || Number.substr(3) == data.firstNumber_3.substr(3)) {
            alert("恭喜中獎! " + Issue + " 四獎: " + data.fourthPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number.substr(4) == data.firstNumber_1.substr(4)
            || Number.substr(4) == data.firstNumber_2.substr(4)
            || Number.substr(4) == data.firstNumber_3.substr(4)) {
            alert("恭喜中獎! " + Issue + " 五獎: " + data.fifthPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        if (Number.substr(5) == data.firstNumber_1.substr(5)
            || Number.substr(5) == data.firstNumber_2.substr(5)
            || Number.substr(5) == data.firstNumber_3.substr(5)) {
            alert("恭喜中獎! " + Issue + " 六獎: " + data.sixthPrize + "元");
            $("#InputNumber").select();
            return false;
        }
        alert("沒有中獎! ");
        $("#InputNumber").select();
        return false;
    });

}

/// 調用API Post包含參數  ///
/// Url : API位址
/// Param: 參數，json格式
/// Callback: 成功後執行方法
/// Error: 失敗後執行方法
function CallApiByParam(Url, Param, Callback, Error) {
    $.ajax({
        url: Url,
        type: 'post',
        dataType: "json",
        async: true,
        contentType: 'application/json',
        data: JSON.stringify(Param),
        success: function (data) {
            Callback(data, Param)
        },
        error: Error()
    });
}

/// 調用API Get 無參數 ///
/// Url : API位址
/// Callback: 成功後執行方法
/// Error: 失敗後執行方法
function CallApi(Url, Callback) {
    $.ajax({
        url: Url,
        success: function (data) {
            Callback(data)
        },
        error: function (jqXHR, textStatus, errorThrow) {
            console.log(errorThrow);
        }
    });
}