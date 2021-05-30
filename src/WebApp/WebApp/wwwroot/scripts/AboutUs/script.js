$(".modalImage")
    .each(function () {
        $(this)
            .click(function () {
                var url = $(this).attr("src");
                url = url.replace("/thumbs", "");
                $(".imagepreview").attr("src", url);
                $("#imagemodal").modal("show");
            });
    });